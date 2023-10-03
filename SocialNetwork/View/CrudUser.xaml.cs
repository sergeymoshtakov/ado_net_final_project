using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SocialNetwork.View
{
    /// <summary>
    /// Логика взаимодействия для CrudUser.xaml
    /// </summary>
    public partial class CrudUser : Window
    {
        private static Mutex? mutex;
        private static String mutexName = "CRUD_MUTEX";
        public Data.Entity.User? User { get; set; }
        public bool IsDeleted { get; set; }
        public bool enabled = false;
        public string InitialName;
        public string InitialSurname;
        public string InitialStatus;
        public string InitialAvatar;
        public CrudUser()
        {
            CheckPreviousLunch();
            InitializeComponent();
            SaveButton.IsEnabled = enabled;
        }

        private void SoftDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            IsDeleted = true;
            DialogResult = true;
        }

        private void HardDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            User = null;
            DialogResult = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            User.Name = NameBox.Text;
            User.Surname = SurnameBox.Text;
            User.Avatar = AvatarBox.Text;
            User.Status = StatusBox.Text;
            if(dobDatePicker.SelectedDate != null)
            {
                User.Birthday = dobDatePicker.SelectedDate.Value;
            }
            if(maleRadioButton.IsChecked == true)
            {
                User.IdGender = new Guid("131ef84b-f06e-494b-848f-bb4bc0604266");
            }
            else
            {
                User.IdGender = new Guid("d3c376e4-bce3-4d85-aba4-e3cf49612c94");
            }
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IdBox.Text = User?.Id.ToString() ?? "";
            NameBox.Text = User?.Name?.ToString() ?? "";
            SurnameBox.Text = User?.Surname?.ToString() ?? "";
            AvatarBox.Text = User?.Avatar?.ToString() ?? "";
            StatusBox.Text = User?.Status ?? "";
            if(User?.IdGender == new Guid("131ef84b-f06e-494b-848f-bb4bc0604266"))
            {
                maleRadioButton.IsChecked = false;
            }
            else
            {
                maleRadioButton.IsChecked = true;
            }
            InitialName = User.Name;
            InitialSurname = User.Surname;
            InitialStatus = User.Avatar ?? "";
            InitialStatus = User.Avatar ?? "";
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            IsDeleted = false;
            User.DeleteDt = null;
            DialogResult = true;
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enabled = NameBox.Text != InitialName;
            SaveButton.IsEnabled = enabled;
        }

        private void SurnameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enabled = SurnameBox.Text != InitialSurname;
            SaveButton.IsEnabled = enabled;
        }

        private void AvatarBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enabled = AvatarBox.Text != InitialAvatar;
            SaveButton.IsEnabled = enabled;
        }

        private void StatusBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enabled = StatusBox.Text != InitialStatus;
            SaveButton.IsEnabled = enabled;
        }
        public void CheckPreviousLunch()
        {
            try
            {
                mutex = Mutex.OpenExisting(mutexName);
            }
            catch { }
            if (mutex != null)
            {
                if (!mutex.WaitOne(1))
                {
                    String message = "Enother exemplar started";
                    MessageBox.Show("Enother exemplar started");
                    throw new Exception(message);
                }

            }
            else
            {
                mutex = new Mutex(true, mutexName);
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            mutex?.ReleaseMutex();
        }
    }
}
