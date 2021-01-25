using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using HoloMetrix.Net.Remote.SoundIntensity;

namespace HoloMetrix_API_Explorer.Dialogs
{
    /// <summary>
    /// Interaction logic for Results_Dialog.xaml
    /// </summary>
    public partial class Results_Dialog : Window
    {
        private Measurement Measurement;

        public Results_Dialog(Measurement measurement)
        {
            InitializeComponent();
            Measurement = measurement;
            TextBox_GroupIndex.Text = measurement.CurrentSegment[0].ToString();
            TextBox_SegmentIndex.Text = measurement.CurrentSegment[1].ToString();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void SetResult_Click(object sender, RoutedEventArgs e)
        {
            var gi = int.Parse(TextBox_GroupIndex.Text);
            var si = int.Parse(TextBox_SegmentIndex.Text);
            Measurement.SetResult(gi, si, float.Parse(TextBox_Result.Text));
        }

        private void TextBox_GroupIndex_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputChecker.IsValid(e.Text, InputChecker.int_Regex);
        }

        private void TextBox_SegmentIndex_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputChecker.IsValid(e.Text, InputChecker.int_Regex);
        }

        private void TextBox_Result_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputChecker.IsValid(e.Text, InputChecker.float_Regex);
        }
    }
}
