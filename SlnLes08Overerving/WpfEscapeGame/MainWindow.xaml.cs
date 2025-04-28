using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfEscapeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum MessageType { Normal, Locked, ItemNotFound }
        Room currentRoom;

        private void UpdateUI()
        {
            lstRoomItems.Items.Clear();
            foreach (Item itm in currentRoom.Items)
            {
                lstRoomItems.Items.Add(itm);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Room room1 = new Room("bedroom", "I seem to be in a medium sized bedroom. There is a locker to the left, a nice rug on the floor, and a bed to the right. ");

            Item key1 = new Item("small silver key", "A small silver key, makes me think of one I hadt highschool. ");
            Item key2 = new Item("large key", "A large key. Could this be my way out? ");
            Item locker = new Item("locker", "A locker. I wonder what's inside. ", false);
            locker.HiddenItem = key2;
            locker.IsLocked = true;
            locker.Key = key1;
            Item bed = new Item("bed", "Just a bed. I am not tired right now. ", false);
            bed.HiddenItem = key1;
             
            Item chair = new Item("chair", "What can i do with this old chair", false);
            Item poster = new Item("Poster", "What can i do with this poster", true);

            room1.Items.Add(new Item("floor mat", "A bit ragged floor mat, but still one of the most popular designs."));
            room1.Items.Add(bed);
            room1.Items.Add(locker);
            room1.Items.Add(chair);
            room1.Items.Add(poster);

            currentRoom = room1;
            lblMessage.Content = "I am awake, but cannot remember who I am!? Must have been a hell of a party last night... ";
            txtRoomDesc.Text = currentRoom.Description;
            UpdateUI();
        }

        private void LstItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnCheck.IsEnabled = lstRoomItems.SelectedValue != null;
            btnPickUp.IsEnabled = lstRoomItems.SelectedValue != null;
            btnUseOn.IsEnabled = lstRoomItems.SelectedValue != null && lstMyItems.SelectedValue != null;
            btnDrop.IsEnabled = lstMyItems.SelectedValue != null;
        }

        private void BtnPickUp_Click(object sender, RoutedEventArgs e)
        {
            Item selItem = (Item)lstRoomItems.SelectedItem;

            if (!selItem.IsPortable) 
            {
                lblMessage.Content = $"i can't take the {selItem.Name} because its to heavy ";
                return;
            }

            lblMessage.Content = $"I just picked up the {selItem.Name}. ";
            lstMyItems.Items.Add(selItem);
            lstRoomItems.Items.Remove(selItem);
            currentRoom.Items.Remove(selItem);
        }

        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            Item roomItem = (Item)lstRoomItems.SelectedItem;

            if (roomItem.IsLocked)
            {
                lblMessage.Content = $"{roomItem.Description}It is firmly locked. ";
                return;
            }

            Item foundItem = roomItem.HiddenItem;
            if (foundItem != null)
            {
                lblMessage.Content = $"Oh, look, I found a {foundItem.Name}. ";
                lstMyItems.Items.Add(foundItem);
                roomItem.HiddenItem = null;
                return;
            }

            lblMessage.Content = roomItem.Description;
        }

        private void BtnUseOn_Click(object sender, RoutedEventArgs e)
        {
            Item myItem = (Item)lstMyItems.SelectedItem;
            Item roomItem = (Item)lstRoomItems.SelectedItem;

            if (roomItem.Key != myItem)
            {
                lblMessage.Content = RandomMessageGenerator.GetRandomMessage(MessageType.Normal);
                return;
            }

            roomItem.IsLocked = false;
            roomItem.Key = null;
            lstMyItems.Items.Remove(myItem);
            lblMessage.Content = $"I just unlocked the {roomItem.Name}!";
        }

        private void BtnDrop_Click(object sender, RoutedEventArgs e)
        {
            Item selItem = (Item)lstMyItems.SelectedItem;

            lstMyItems.Items.Remove(selItem);
            currentRoom.Items.Add(selItem);

            lblMessage.Content = $"i just drop {selItem.Name} back in the room.";
            UpdateUI(); 
        }
    }
}