using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SocialNetwork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Data.DataContext dataContext;
        public ObservableCollection<Pair> Pairs { get; set; }
        public ObservableCollection<Data.Entity.User> UsersView { get; set; }
        private ICollectionView usersListView;
        public MainWindow()
        {
            InitializeComponent();
            dataContext = new Data.DataContext();
            Pairs = new ObservableCollection<Pair>();
            UsersView = new ObservableCollection<Data.Entity.User>();
            this.DataContext = this;
        }
        public class Pair
        {
            public String Key { get; set; } = null!;
            public String? Value { get; set; }
        }
        private void GenderButton_Click(object sender, RoutedEventArgs e)
        {
            var query = dataContext
                .Users
                .Where(u => u.DeleteDt == null)
                .Select(m => new Pair() { Key = $"{m.Surname} {m.Name[0]}.", Value = m.Gender.Name });
            Pairs.Clear();
            foreach (var pair in query)
            {
                Pairs.Add(pair);
            }
        }


        private void AgeButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var query = dataContext.Users
                .Where(u => u.DeleteDt == null)
                .Join(dataContext.Users, m1 => m1.Id, m2 => m2.Id, (m1, m2) => new Pair()
                {
                    Key = $"{m1.Surname} {m1.Name[0]}.",
                    Value = (today.Year - m2.Birthday.Year).ToString() // calculate age based on current date
                })
                .ToList()
                .OrderByDescending(d => int.Parse(d.Value));

            Pairs.Clear();
            foreach (var pair in query)
            {
                Pairs.Add(pair);
            }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                if (item.Content is Data.Entity.User user)
                {
                    View.CrudUser dialog = new() { User = user };
                    if (dialog.ShowDialog() ?? false)
                    {
                        if (dialog.User != null)
                        {
                            if (dialog.IsDeleted)
                            {
                                var dep = dataContext.Users.Find(user.Id);
                                user.DeleteDt = DateTime.Now;
                                dataContext.SaveChanges();
                                usersListView.Refresh();
                            }
                            else
                            {
                                var dep = dataContext.Users.Find(user.Id);
                                if (dep != null)
                                {
                                    dep.Name = user.Name;   
                                }
                                dataContext.SaveChanges();
                                usersListView.Refresh();
                                // int index = UsersView.IndexOf(user);
                                // UsersView.Remove(user);
                                // UsersView.Insert(index, user);
                            }
                        }
                        else // dialog.Department == null deleted
                        {
                            dataContext.Users.Remove(user);
                            dataContext.SaveChanges();
                            usersListView.Refresh();
                        }
                    }
                }
            }
        }

        private async void ShowDeleted_Checked(object sender, RoutedEventArgs e)
        {
            await ApplyFilterAsync(item => (item as Data.Entity.User)?.DeleteDt == null || (item as Data.Entity.User)?.DeleteDt != null);
        }

        private async void ShowDeleted_Unchecked(object sender, RoutedEventArgs e)
        {
            await ApplyFilterAsync(item => (item as Data.Entity.User)?.DeleteDt == null);
        }

        private async void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            await AddNewUserAsync();
        }

        private async Task AddNewUserAsync()
        {
            try
            {
                Data.Entity.User newUser = new() { Id = new Guid(), Name = " ", Surname = "", Birthday = DateTime.Now, CreateDt = DateTime.Now, Status = null, Avatar = null, Gender = null, DeleteDt = null, IdGender = new Guid("d3c376e4-bce3-4d85-aba4-e3cf49612c94") };
                View.CrudUser dialog = new() { User = newUser };
                if (dialog.ShowDialog() ?? false)
                {
                    dataContext.Users.Add(newUser);
                    await dataContext.SaveChangesAsync();
                    await FetchUsersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving user: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task FetchUsersAsync()
        {
            try
            {
                await dataContext.Users.LoadAsync();
                UsersView = dataContext.Users.Local.ToObservableCollection();
                usersList.ItemsSource = UsersView;
                usersListView = CollectionViewSource.GetDefaultView(UsersView);
                usersListView.Filter = item => (item as Data.Entity.User)?.DeleteDt == null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while fetching users: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await FetchUsersAsync();
        }

        private void RegisteredButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var query = dataContext.Users.Where(u => u.DeleteDt == null).Join(dataContext.Users, m1 => m1.Id, m2 => m2.Id,
                (m1, m2) => new Pair()
                {
                    Key = $"{m1.Surname} {m1.Name[0]}.",
                    Value = (today - m2.CreateDt).Days.ToString()
                })
                .ToList()
                .OrderByDescending(d => int.Parse(d.Value));

            Pairs.Clear();
            foreach (var pair in query)
            {
                pair.Value = pair.Value + " days ago";
                Pairs.Add(pair);
            }
        }

        private void GenderStatisticButton_Click(object sender, RoutedEventArgs e)
        {
            var query = dataContext.Genders.Select(d => new Pair()
            {
                Key = d.Name,
                Value = d.Users.Where(u => u.DeleteDt == null).Count().ToString()
            }).ToList()
                .OrderByDescending(pair => int.Parse(pair.Value));

            Pairs.Clear();
            foreach (var pair in query)
            {
                if (pair.Value == "0")
                {
                    pair.Value = "closed";
                }
                Pairs.Add(pair);
            }
        }

        private async Task ApplyFilterAsync(Predicate<object> filter)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                usersListView.Filter = item => filter(item);
                usersListView.Refresh();
            });
        }
    }
}
