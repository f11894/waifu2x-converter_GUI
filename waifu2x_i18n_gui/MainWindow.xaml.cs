using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.ComponentModel; // CancelEventArgs

namespace waifu2x_i18n_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var dirInfo = new DirectoryInfo(App.directory);
            var langlist = dirInfo.GetFiles("UILang.*.xaml");
            string[] langcodelist = new string[langlist.Length];
            for (int i = 0; i < langlist.Length; i++)
            {
                var fn_parts = langlist[i].ToString().Split('.');
                langcodelist[i] = fn_parts[1];
            }

            foreach (var langcode in langcodelist)
            {
                MenuItem mi = new MenuItem();
                mi.Tag = langcode;
                mi.Header = langcode;
                mi.Click += new RoutedEventHandler(MenuItem_Style_Click);
                menuLang.Items.Add(mi);
            }
            foreach (MenuItem item in menuLang.Items)
            {
                if (item.Tag.ToString().Equals(CultureInfo.CurrentUICulture.Name))
                {
                    item.IsChecked = true;
                }
            }
            // 設定をファイルから読み込む
            txtExt.Text = Properties.Settings.Default.informat;

            if (System.Text.RegularExpressions.Regex.IsMatch(
                Properties.Settings.Default.Device_ID,
                @"^(\d+|-1)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                txtDevice.Text = Properties.Settings.Default.Device_ID;
            }
            

            btn512.IsChecked = true;

            if (Properties.Settings.Default.block_size == "512")
            { btn512.IsChecked = true; }
            if (Properties.Settings.Default.block_size == "256")
            { btn256.IsChecked = true; }
            if (Properties.Settings.Default.block_size == "128")
            { btn128.IsChecked = true; }
            if (Properties.Settings.Default.block_size == "64")
            { btn64.IsChecked = true; }

            //btnCUDA.IsChecked = true;
            btnDenoise0.IsChecked = true;

            if (Properties.Settings.Default.noise_level == "3")
            { btnDenoise3.IsChecked = true; }
            if (Properties.Settings.Default.noise_level == "2")
            { btnDenoise2.IsChecked = true; }
            if (Properties.Settings.Default.noise_level == "1")
            { btnDenoise1.IsChecked = true; }
            if (Properties.Settings.Default.noise_level == "0")
            { btnDenoise0.IsChecked = true; }

            btnRGB.IsChecked = true;

            if (Properties.Settings.Default.model_dir == "RGB")
            { btnRGB.IsChecked = true; }
            if (Properties.Settings.Default.model_dir == "Y")
            { btnY.IsChecked = true; }
            if (Properties.Settings.Default.model_dir == "Photo")
            { btnPhoto.IsChecked = true; }

            btnModeScale.IsChecked = true;

            if (Properties.Settings.Default.mode == "scale")
            { btnModeScale.IsChecked = true; }
            if (Properties.Settings.Default.mode == "noise_scale")
            { btnModeNoiseScale.IsChecked = true; }
            if (Properties.Settings.Default.mode == "noise")
            { btnModeNoise.IsChecked = true; }
            if (Properties.Settings.Default.mode == "auto_scale")
            { btnModeAutoScale.IsChecked = true; }

            output_width.Clear();

            if (Properties.Settings.Default.output_width != 0)
            { output_width.Text = Properties.Settings.Default.output_width.ToString(); }

            output_height.Clear();

            if (Properties.Settings.Default.output_height != 0)
            { output_height.Text = Properties.Settings.Default.output_height.ToString(); }

            if (this.output_width.Text.Trim() != "")
            {
                slider_zoom.IsEnabled = false;
                slider_value.IsEnabled = false;
            }

            if (this.output_height.Text.Trim() != "")
            {
                slider_zoom.IsEnabled = false;
                slider_value.IsEnabled = false;
            }

            { checkDandD.IsChecked = false; }

            if ( Properties.Settings.Default.DandD_check == true )
            { checkDandD.IsChecked = true; }

            { checkAspect_ratio_keep.IsChecked = false; }

            if (Properties.Settings.Default.Aspect_ratio_keep == true)
            { checkAspect_ratio_keep.IsChecked = true; }

            { checkSoundBeep.IsChecked = false; }

            if (Properties.Settings.Default.SoundBeep == true)
            { checkSoundBeep.IsChecked = true; }

            slider_value.Text = Properties.Settings.Default.scale_ratio;
            slider_zoom.Value = double.Parse(Properties.Settings.Default.scale_ratio);

            //cbTTA.IsChecked = false;

        }
        public static StringBuilder waifu2xbinary = new StringBuilder("");

        public static StringBuilder param_src= new StringBuilder("");
        public static StringBuilder param_dst = new StringBuilder("");
        public static StringBuilder param_dst_dd = new StringBuilder("");
        public static StringBuilder param_informat = new StringBuilder("*.jpg *.jpeg *.png *.bmp *.tif *.tiff");
        //public static StringBuilder param_outformat = new StringBuilder("png");
        public static StringBuilder param_mag = new StringBuilder("2");
        public static StringBuilder param_denoise_temp= new StringBuilder("");
        public static StringBuilder param_denoise = new StringBuilder("");
        public static StringBuilder param_color = new StringBuilder(@"--model_dir models_rgb");
        public static StringBuilder param_model = new StringBuilder(@"--model_dir models_rgb");
        //public static StringBuilder param_device = new StringBuilder("-p gpu");
        public static StringBuilder param_block = new StringBuilder("--block_size 512");
        public static StringBuilder param_mode = new StringBuilder("noise_scale");
        public static StringBuilder param_device = new StringBuilder("");
        public static StringBuilder random32 = new StringBuilder("");
        public static StringBuilder Not_Aspect_ratio_keep_argument = new StringBuilder("");
        public static StringBuilder Aspect_ratio_keep_argument = new StringBuilder("");

        public static bool DandD_Mode = false;
        public static int FileCount = (0);

        //public static StringBuilder param_tta = new StringBuilder("-t 0");
        public static Process pHandle = new Process();
        public static ProcessStartInfo psinfo = new ProcessStartInfo();

        public static StringBuilder console_buffer = new StringBuilder();
        public static StringBuilder waifu2x_bat = new StringBuilder("");
        public static bool flagAbort = false;

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
                
            // 設定を保存
            if (txtExt.Text.Trim() != "")
            {
               Properties.Settings.Default.informat = txtExt.Text;
            } else
            {
               Properties.Settings.Default.informat = "*.jpg *.jpeg *.png *.bmp *.tif *.tiff";
            }
            // Properties.Settings.Default.Device_ID = txtDevice.Text;
            
            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtDevice.Text,
                @"^(\d+|-1)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
               Properties.Settings.Default.Device_ID = txtDevice.Text;
            } else 
            {
               Properties.Settings.Default.Device_ID = "Unspecified";
            }
            
            if (param_block.ToString().Trim() == "--block_size 512")
            Properties.Settings.Default.block_size = "512";
            if (param_block.ToString().Trim() == "--block_size 256")
            Properties.Settings.Default.block_size = "256";
            if (param_block.ToString().Trim() == "--block_size 128")
            Properties.Settings.Default.block_size = "128";            
            if (param_block.ToString().Trim() == "--block_size 64")
            Properties.Settings.Default.block_size = "64";
            
            if (param_denoise_temp.ToString().Trim() == "--noise_level 3")
            { Properties.Settings.Default.noise_level = "3"; }
            if (param_denoise_temp.ToString().Trim() == "--noise_level 2")
            { Properties.Settings.Default.noise_level = "2"; }
            if (param_denoise_temp.ToString().Trim() == "--noise_level 1")
            { Properties.Settings.Default.noise_level = "1"; }
            if (param_denoise_temp.ToString().Trim() == "--noise_level 0")
            { Properties.Settings.Default.noise_level = "0"; }
            
            if (param_color.ToString().Trim() == "--model_dir models_rgb")
            {Properties.Settings.Default.model_dir = "RGB";}
            if (param_color.ToString().Trim() == "--model_dir models")
            {Properties.Settings.Default.model_dir = "Y";}
            if (param_color.ToString().Trim() == "--model_dir photo")
            {Properties.Settings.Default.model_dir = "Photo";}

            Properties.Settings.Default.mode = param_mode.ToString();

            if (checkDandD.IsChecked == true)
            { Properties.Settings.Default.DandD_check = true; }
            if (checkDandD.IsChecked == false)
            { Properties.Settings.Default.DandD_check = false; }

            if (checkAspect_ratio_keep.IsChecked == true)
            { Properties.Settings.Default.Aspect_ratio_keep = true; }
            if (checkAspect_ratio_keep.IsChecked == false)
            { Properties.Settings.Default.Aspect_ratio_keep = false; }

            if (checkSoundBeep.IsChecked == true)
            { Properties.Settings.Default.SoundBeep = true; }
            if (checkSoundBeep.IsChecked == false)
            { Properties.Settings.Default.SoundBeep = false; }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                slider_value.Text,
                @"^\d+(\.\d+)?$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
               Properties.Settings.Default.scale_ratio = slider_value.Text;
            } else 
            {
               Properties.Settings.Default.scale_ratio = "2";
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                output_width.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.output_width = double.Parse(output_width.Text);
            }
            else
            {
                Properties.Settings.Default.output_width = 0;
            }


            if (System.Text.RegularExpressions.Regex.IsMatch(
                output_height.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.output_height = double.Parse(output_height.Text);
            }
            else
            {
                Properties.Settings.Default.output_height = 0;
            }

            Properties.Settings.Default.Save();

            try
            {
                pHandle.Kill();
            }
            catch (Exception) { /*Nothing*/ }

            if (waifu2x_bat.ToString() != "")
            {
                if (File.Exists(waifu2x_bat.ToString()))
                { System.IO.File.Delete(@waifu2x_bat.ToString()); }
            }
        }

        private void OnMenuHelpClick(object sender, RoutedEventArgs e)
        {
            string msg =
                "This is a multilingual graphical user-interface\n" +
                "for the waifu2x-converter commandline program.\n" +
                "You need a working copy of waifu2x-converter first\n" +
                "then copy everything from the GUI archive to\n" +
                "waifu2x-converter folder.\n" +
                "DO NOT rename any subdirectories inside waifu2x-converter folder\n" +
                "To make a translation, copy one of the bundled xaml file\n" +
                "then edit the copy with a text editor.\n" +
                "Whenever you see a language code like en-US, change it to\n" +
                "the target language code like zh-TW, ja-JP.\n" +
                "The filename needs to be changed too.";
            MessageBox.Show(msg);
        }

        private void OnMenuVersionClick(object sender, RoutedEventArgs e)
        {
            string msg =
                "Multilingual GUI for waifu2x-converter\n" +
                "nanashi (2018)\n" +
                "Version 1.5.4\n" +
                "BuildDate: 4 Jan,2018\n" +
                "License: Do What the Fuck You Want License";
            MessageBox.Show(msg);
        }

        private void OnBtnSrc(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fdlg= new OpenFileDialog();
            if (fdlg.ShowDialog() == true)
            {
                this.txtSrcPath.Text = fdlg.FileName;
            }
        }

        private void OnSrcClear(object sender, RoutedEventArgs e)
        {
            this.txtSrcPath.Clear();
        }

        private void OnBtnDst(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fdlg = new SaveFileDialog();
            fdlg.Filter = "PNG Image | *.png";
            fdlg.DefaultExt = "png";
            if (fdlg.ShowDialog() == true)
            {
                this.txtDstPath.Text = fdlg.FileName;
            }
        }

        private void OnDstClear(object sender, RoutedEventArgs e)
        {
            this.txtDstPath.Clear();
        }

        private void OnFormatReset(object sender, RoutedEventArgs e)
        {
            this.txtExt.Text = "*.jpg *.jpeg *.png *.bmp *.tif *.tiff";
        }

        private void MenuItem_Style_Click(object sender, RoutedEventArgs e)
        {
            foreach(MenuItem item in menuLang.Items)
            {
                item.IsChecked = false;
            }
            MenuItem mi = (MenuItem)sender;
            mi.IsChecked = true;
            App.Instance.SwitchLanguage(mi.Tag.ToString());
        }

        private void On_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects= DragDropEffects.None;
            }
            
            e.Handled = true;
        }

        private void On_SrcDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fn = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (checkDandD.IsChecked == false)
                { this.txtSrcPath.Text = fn[0];}
                if (checkDandD.IsChecked == true) if (DandD_Mode == false)
                {
                    DandD_Mode = true;
                    this.txtSrcPath.Clear();
                    Guid g = System.Guid.NewGuid();
                    random32.Clear();
                    random32.Append(g.ToString("N").Substring(0, 32));
                    waifu2x_bat.Clear();
                    waifu2x_bat.Append("waifu2x_" + random32.ToString() + ".bat");
                    StreamWriter sw = new System.IO.StreamWriter(
                               @waifu2x_bat.ToString(),
                               false
                               // ,
                               // System.Text.Encoding.GetEncoding("utf-8")
                               );
                    sw.WriteLine("@echo off");
                    sw.WriteLine("chcp 65001 >nul");
                    sw.WriteLine("set \"ProcessedCount=0\"");
                    FileCount = 0;
                    for (int i = 0; i < fn.Length; i++)
                    {
                        FileCount ++ ;
                        string list = fn[i];
                        string list2 = list.Replace("%", "%%");
                        sw.WriteLine("set list_path=\"" + list2 + "\"&&call :list_Allocation");
                    }
                    sw.Close(); ;
                    OnRun(sender,e);
                }
            }
        }

        private void On_DstDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fn = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.txtDstPath.Text = fn[0];
            }
        }

        private void OnSetModeChecked(object sender, RoutedEventArgs e)
        {
            gpDenoise.IsEnabled = true;
            param_mode.Clear();
            RadioButton optsrc = sender as RadioButton;
            param_mode.Append(optsrc.Tag.ToString());
            if (btnModeScale.IsChecked == true)
            { gpDenoise.IsEnabled = false;}
        }

        private void OutputSize_TextChanged(object sender, EventArgs e)
        {

            if (this.output_width.Text.Trim() != "")
                {
                slider_zoom.IsEnabled = false;
                slider_value.IsEnabled = false;
            }

            if (this.output_height.Text.Trim() != "")
            {
                slider_zoom.IsEnabled = false;
                slider_value.IsEnabled = false;
            }
            if (this.output_height.Text.Trim() == "") if (this.output_width.Text.Trim() == "")
                {
                    slider_zoom.IsEnabled = true;
                    slider_value.IsEnabled = true;
                }
        }

        private void OnDenoiseChecked(object sender, RoutedEventArgs e)
        {
            param_denoise_temp.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_denoise_temp.Append(optsrc.Tag.ToString());
        }

        private void OnColorChecked(object sender, RoutedEventArgs e)
        {
            param_color.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_color.Append(optsrc.Tag.ToString());
        }

        /*private void OnDeviceChecked(object sender, RoutedEventArgs e)
        {
            param_device.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_device.Append(optsrc.Tag.ToString());
        }
        */

        private void OnBlockChecked(object sender, RoutedEventArgs e)
        {
            param_block.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_block.Append(optsrc.Tag.ToString());
        }

        /*private void OnTTAChecked(object sender, RoutedEventArgs e)
        {
            param_tta.Clear();
            CheckBox optsrc= sender as CheckBox;
            if (optsrc.IsChecked.Value)
            {
                param_tta.Append(optsrc.Tag.ToString());
            }
            
        }
        */
        private void OnConsoleDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                console_buffer.Append(e.Data);
                console_buffer.Append(Environment.NewLine);
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    this.CLIOutput.Clear();
                    this.CLIOutput.AppendText(e.Data);
                    this.CLIOutput.AppendText(Environment.NewLine);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle, null);
            }
            
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            if (!flagAbort)
            {
                try
                {
                    pHandle.CancelOutputRead();
                }
                catch (Exception)
                {
                    //No need to throw
                    //throw;
                }
                
            }
            
            pHandle.Close();
            Dispatcher.BeginInvoke(new Action(delegate
            {
                this.btnAbort.IsEnabled = false;
                this.btnRun.IsEnabled = true;
                //this.CLIOutput.Text = console_buffer.ToString();

                if (checkSoundBeep.IsChecked == true)
                { System.Media.SystemSounds.Beep.Play(); }

            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle, null);
            flagAbort = false;
            DandD_Mode = false;
        }

        private void OnAbort(object sender, RoutedEventArgs e)
        {
            if (!pHandle.HasExited)
            {
                try
                {
                    pHandle.CancelOutputRead();
                }
                catch (Exception) { /*Nothing*/ }
                pHandle.Kill();

                    if (waifu2x_bat.ToString() != "")
                    {
                        if (File.Exists(waifu2x_bat.ToString()))
                        { System.IO.File.Delete(@waifu2x_bat.ToString()); }
                    }


                flagAbort = true;
                this.CLIOutput.Clear();
            }
        }

        private void OnRun(object sender, RoutedEventArgs e)
        {
            // Simple checks before further execution //
            if (File.Exists("waifu2x-converter_x86.exe"))
            {
                waifu2xbinary.Clear();
                waifu2xbinary.Append("waifu2x-converter_x86.exe");
            }
            if (File.Exists("waifu2x-converter_x64.exe"))
            {
                waifu2xbinary.Clear();
                waifu2xbinary.Append("waifu2x-converter_x64.exe");
            }
            if (File.Exists("waifu2x-converter-cpp.exe"))
            {
                waifu2xbinary.Clear();
                waifu2xbinary.Append("waifu2x-converter-cpp.exe");
            }
            if (waifu2xbinary.ToString() == "")
            {
                MessageBox.Show(@"waifu2x-converter is missing!");
                return;
            }
                /*if (!File.Exists("waifu2x-converter_x64.exe"))
                {
                    MessageBox.Show(@"waifu2x-converter_x64.exe is missing!");
                    return;
                }*/

                if (param_color.ToString() == "--model_dir models_rgb")
            {
                if (!Directory.Exists("models_rgb"))
                {
                    MessageBox.Show("Training models_rgb model folder is missing!");
                    return;
                }
            }
            if (param_color.ToString() == "--model_dir models")
            {
                if (!Directory.Exists("models"))
                {
                    MessageBox.Show("Training models model folder is missing!");
                    return;
                }
            }
            if (param_color.ToString() == "--model_dir photo")
            {
                if (!Directory.Exists("photo"))
                {
                    MessageBox.Show("Training photo model folder is missing!");
                    return;
                }
            }

            /*
            if (!Directory.Exists("models"))
            {
                MessageBox.Show("Training model folder is missing!");
                return;
            }
            */

            // Sets Source
            // The source must be a file or folder that exists
            if (DandD_Mode == false) if (File.Exists(this.txtSrcPath.Text) || Directory.Exists(this.txtSrcPath.Text))
            {
                if (this.txtSrcPath.Text.Trim() == "") //When source path is empty, replace with current folder
                {
                    param_src.Clear();
                    //param_src.Append("-i ");
                    //param_src.Append("\"");
                    param_src.Append(App.directory);
                    //param_src.Append("\"");
                }
                else
                {
                    param_src.Clear();
                    //param_src.Append("-i ");
                    //param_src.Append("\"");
                    param_src.Append(this.txtSrcPath.Text);
                    //param_src.Append("\"");
                }
            }
            else
            {
                MessageBox.Show(@"The source folder or file does not exists!");
                return;
            }

            // D&D処理時に出力先フォルダが見つからなければ出力先をクリアする
            if (DandD_Mode == true)
            {
                if (this.txtDstPath.Text.Trim() != "") if (Directory.Exists(this.txtDstPath.Text)==false)
                {
                    this.txtDstPath.Clear();
                }

            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(
                output_width.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                output_width.Clear();
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(
                output_height.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                output_height.Clear();
            }

            // 縦横比を保たない引数を追加する
            Not_Aspect_ratio_keep_argument.Clear();
            if (checkAspect_ratio_keep.IsChecked == false)
            { Not_Aspect_ratio_keep_argument.Append("!"); }

            // 縦横比を保つ引数を追加する
            Aspect_ratio_keep_argument.Clear();
            if (this.output_width.Text.Trim() != "") if (this.output_height.Text.Trim() != "") if (checkAspect_ratio_keep.IsChecked == true)
            { Aspect_ratio_keep_argument.Append("1"); }

            // Set Destination
            param_dst_dd.Clear();
            if (this.txtDstPath.Text.Trim() == "")
            {
                //

                param_dst.Clear();
                // nanashi Append
                //param_dst.Append("-o ");
                if (DandD_Mode == false)
                {
                    if (File.Exists(this.txtSrcPath.Text))
                    { param_dst.Append("\""); }
                    System.IO.DirectoryInfo hDirInfo = System.IO.Directory.GetParent(this.txtSrcPath.Text);
                    param_dst.Append(hDirInfo.FullName);
                    param_dst.Append("\\");
                    if (File.Exists(this.txtSrcPath.Text))
                    { param_dst.Append(System.IO.Path.GetFileNameWithoutExtension(this.txtSrcPath.Text)); }
                    if (Directory.Exists(this.txtSrcPath.Text))
                    { param_dst.Append(System.IO.Path.GetFileName(this.txtSrcPath.Text)); }
                }

                if (param_color.ToString() == "--model_dir models_rgb")
                   { param_dst.Append("(RGB)"); }
                    if (param_color.ToString() == "--model_dir models")
                    { param_dst.Append("(Y)"); }
                    if (param_color.ToString() == "--model_dir photo")
                    { param_dst.Append("(Photo)"); }
            }
            else
            {
                param_dst.Clear();
                //param_dst.Append("-o ");
                if (DandD_Mode == false)
                {
                    if (File.Exists(this.txtSrcPath.Text))
                    { param_dst.Append("\""); }
                    
                    param_dst.Append(this.txtDstPath.Text);
                    
                    if (Directory.Exists(this.txtSrcPath.Text))
                    { param_dst.Append("\\"); }
                    
                    // 入力先が画像で出力先がフォルダの場合
                    if (File.Exists(this.txtSrcPath.Text)) if (Directory.Exists(this.txtDstPath.Text))
                    {
                      param_dst.Append("\\");
                      param_dst.Append(System.IO.Path.GetFileNameWithoutExtension(this.txtSrcPath.Text)); 
                      param_dst.Append(".png");
                    }
                    
                    if (File.Exists(this.txtSrcPath.Text))
                    { param_dst.Append("\""); }
                }

                if (DandD_Mode == true)
                {
                    if (Directory.Exists(this.txtDstPath.Text))
                    {
                        param_dst_dd.Append(System.IO.Path.GetFullPath(this.txtDstPath.Text));
                        param_dst_dd.Replace("%", "%%");
                        param_dst_dd.Append("\\");
                    }
                }
            }

            // Set input format
            param_informat.Clear();
            //param_informat.Append("-l ");
            param_informat.Append(this.txtExt.Text);
            //param_informat.Append(@":");
            //param_informat.Append(this.txtExt.Text.ToUpper());

            // Set output format
            //param_outformat.Clear();
            //param_outformat.Append("-e ");
            //param_outformat.Append(this.txtOExt.Text);

            // Set scale ratio

            param_mag.Clear();
            param_mag.Append("--scale_ratio ");
            param_mag.Append("%scale_ratio%");



            // Set mode
            if (param_denoise_temp.ToString() == "--noise_level 0")
            {
                param_model.Clear();
                param_model.Append(param_color.ToString());
                param_model.Append("2");
                param_denoise.Clear();
                param_denoise.Append("--noise_level 1");
            }

            if (param_denoise_temp.ToString() == "--noise_level 1")
            {
                param_model.Clear();
                param_model.Append(param_color.ToString());
                param_denoise.Clear();
                param_denoise.Append("--noise_level 1");
            }

            if (param_denoise_temp.ToString() == "--noise_level 2")
            {
                param_model.Clear();
                param_model.Append(param_color.ToString());
                param_denoise.Clear();
                param_denoise.Append("--noise_level 2");
            }

            if (param_denoise_temp.ToString() == "--noise_level 3")
            {
                param_model.Clear();
                param_model.Append(param_color.ToString());
                param_model.Append("2");
                param_denoise.Clear();
                param_denoise.Append("--noise_level 2");
            }

            if (param_mode.ToString() == "noise_scale")
            {
                if (this.txtDstPath.Text.Trim() == "")
                {
                    param_dst.Append("(noise_scale)");

                    if (param_denoise_temp.ToString() == "--noise_level 0")
                    { param_dst.Append("(Level0)"); }
                    if (param_denoise_temp.ToString() == "--noise_level 1")
                    { param_dst.Append("(Level1)"); }
                    if (param_denoise_temp.ToString() == "--noise_level 2")
                    { param_dst.Append("(Level2)"); }
                    if (param_denoise_temp.ToString() == "--noise_level 3")
                    { param_dst.Append("(Level3)"); }

                    if (this.output_width.Text.Trim() == "") if (this.output_height.Text.Trim() == "")
                        {
                            param_dst.Append("(x");
                            param_dst.Append(this.slider_zoom.Value.ToString());
                            param_dst.Append(")");
                        }
                    if (this.output_width.Text.Trim() != "") if (this.output_height.Text.Trim() == "")
                        {
                            param_dst.Append("(width ");
                            param_dst.Append(this.output_width.Text);
                            param_dst.Append(")");
                        }
                    if (this.output_width.Text.Trim() == "") if (this.output_height.Text.Trim() != "")
                        {
                            param_dst.Append("(height ");
                            param_dst.Append(this.output_height.Text);
                            param_dst.Append(")");
                        }
                    if (this.output_width.Text.Trim() != "") if (this.output_height.Text.Trim() != "")
                        {
                            param_dst.Append("(");
                            if (checkAspect_ratio_keep.IsChecked == true)
                            { param_dst.Append("within "); }

                            param_dst.Append(this.output_width.Text);
                            param_dst.Append("x");
                            param_dst.Append(this.output_height.Text);
                            param_dst.Append(")");
                        }
                }
            }
            if (param_mode.ToString() == "auto_scale")
            {
                if (this.txtDstPath.Text.Trim() == "")
                {
                    param_dst.Append("(auto_scale)");

                    if (param_denoise_temp.ToString() == "--noise_level 0")
                    { param_dst.Append("(Level0)"); }
                    if (param_denoise_temp.ToString() == "--noise_level 1")
                    { param_dst.Append("(Level1)"); }
                    if (param_denoise_temp.ToString() == "--noise_level 2")
                    { param_dst.Append("(Level2)"); }
                    if (param_denoise_temp.ToString() == "--noise_level 3")
                    { param_dst.Append("(Level3)"); }

                    if (this.output_width.Text.Trim() == "") if (this.output_height.Text.Trim() == "")
                        {
                            param_dst.Append("(x");
                            param_dst.Append(this.slider_zoom.Value.ToString());
                            param_dst.Append(")");
                        }
                    if (this.output_width.Text.Trim() != "") if (this.output_height.Text.Trim() == "")
                        {
                            param_dst.Append("(width ");
                            param_dst.Append(this.output_width.Text);
                            param_dst.Append(")");
                        }
                    if (this.output_width.Text.Trim() == "") if (this.output_height.Text.Trim() != "")
                        {
                            param_dst.Append("(height ");
                            param_dst.Append(this.output_height.Text);
                            param_dst.Append(")");
                        }
                    if (this.output_width.Text.Trim() != "") if (this.output_height.Text.Trim() != "")
                        {
                            param_dst.Append("(");
                            if (checkAspect_ratio_keep.IsChecked == true)
                            { param_dst.Append("within "); }

                            param_dst.Append(this.output_width.Text);
                            param_dst.Append("x");
                            param_dst.Append(this.output_height.Text);
                            param_dst.Append(")");
                        }
                }
            }
            if (param_mode.ToString() == "noise")
            {
                param_mag.Clear();
                if (this.txtDstPath.Text.Trim() == "")
                {
                    param_dst.Append("(noise)");

                    if (param_denoise_temp.ToString() == "--noise_level 0")
                    { param_dst.Append("(Level0)"); }
                    if (param_denoise_temp.ToString() == "--noise_level 1")
                    { param_dst.Append("(Level1)"); }
                    if (param_denoise_temp.ToString() == "--noise_level 2")
                    { param_dst.Append("(Level2)"); }
                    if (param_denoise_temp.ToString() == "--noise_level 3")
                    { param_dst.Append("(Level3)"); }
                }
            }
            if (param_mode.ToString() == "scale")
            {
                param_model.Clear();
                param_model.Append(param_color.ToString());
                param_denoise.Clear();
                if (this.txtDstPath.Text.Trim() == "")
                {
                    param_dst.Append("(scale)");
                    if (this.output_width.Text.Trim() == "") if (this.output_height.Text.Trim() == "")
                        {
                        param_dst.Append("(x");
                        param_dst.Append(this.slider_zoom.Value.ToString());
                        param_dst.Append(")");
                    }
                    if (this.output_width.Text.Trim() != "") if (this.output_height.Text.Trim() == "")
                        {
                            param_dst.Append("(width ");
                            param_dst.Append(this.output_width.Text);
                            param_dst.Append(")");
                        }
                    if (this.output_width.Text.Trim() == "") if (this.output_height.Text.Trim() != "")
                        {
                            param_dst.Append("(height ");
                            param_dst.Append(this.output_height.Text);
                            param_dst.Append(")");
                        }
                    if (this.output_width.Text.Trim() != "") if (this.output_height.Text.Trim() != "")
                        {
                            param_dst.Append("(");
                            if (checkAspect_ratio_keep.IsChecked == true)
                            { param_dst.Append("within "); }

                            param_dst.Append(this.output_width.Text);
                            param_dst.Append("x");
                            param_dst.Append(this.output_height.Text);
                            param_dst.Append(")");
                        }
                }
            }

            if (DandD_Mode == false) if (this.txtDstPath.Text.Trim() == "")
            {
                if (File.Exists(this.txtSrcPath.Text))
                { param_dst.Append(".png\""); }
                if (Directory.Exists(this.txtSrcPath.Text))
                { param_dst.Append("\\"); }
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtDevice.Text,
                @"^(\d+|-1)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            { 
              param_device.Clear();
              param_device.Append("--processor ");
              param_device.Append(txtDevice.Text);
            } else
            {
                param_device.Clear();
            }

            this.btnRun.IsEnabled = false;
            this.btnAbort.IsEnabled = true;

            if (DandD_Mode == false)
            {
                Guid g = System.Guid.NewGuid();
                random32.Clear();
                random32.Append(g.ToString("N").Substring(0, 32));
                waifu2x_bat.Clear();
                waifu2x_bat.Append("waifu2x_" + random32.ToString() + ".bat");
            }
            // %をエスケープする
            param_src.Replace("%", "%%");
            param_dst.Replace("%", "%%");

            if (DandD_Mode == false) if (File.Exists(this.txtSrcPath.Text))
            {
                // Assemble parameters
                string TextBox1 = "@echo off\r\n" +
                "chcp 65001 >nul\r\n" +
                 "set Image_path=\"" + param_src.ToString() + "\"\r\n" +
                 ":waifu2x_run\r\n" +
                 "setlocal\r\n" +
                 "FOR %%A IN (" + param_dst.ToString() + ") DO set \"OUTPUT_Name=%%~nA\"\r\n" +
                 "FOR %%A IN (" + param_dst.ToString() + ") DO set \"Output_dir=%%~dpA\"\r\n" +


                 // bat共通の処理
                 "set \"keep_aspect_ratio=" + Aspect_ratio_keep_argument.ToString() + "\"\r\n" + 
                 "set \"scale_ratio=" + this.slider_zoom.Value.ToString() + "\"\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"noise\" set \"output_width=" + this.output_width.Text + "\"\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"noise\" set \"output_height=" + this.output_height.Text + "\"\r\n" +
                 "FOR %%A IN (%Image_path%) DO set \"Image_ext=%%~xA\"\r\n" +
                 "if /i \"%Image_ext%\"==\".png\" identify.exe -format \"%%A\" %Image_path% | find \"Blend\"> NUL && set image_alpha=true\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"noise\" if not \"%output_width%%output_height%\"==\"\" (\r\n" +
                 "   for /f \"delims=\" %%a in ('identify.exe -format \"%%w\" %Image_path%') do set \"image_width=%%a\"\r\n" +
                 "   for /f \"delims=\" %%a in ('identify.exe -format \"%%h\" %Image_path%') do set \"image_height=%%a\"\r\n" +
                 "   set scale_ratio=1\r\n" +
                 "   call :scale_ratio_set\r\n" +
                 ")\r\n" +
                 "if \"" + param_mode.ToString() + "\"==\"auto_scale\" (\r\n" +
                 "   if /i \"%Image_ext%\"==\".jpg\" set jpg=1\r\n" +
                 "   if /i \"%Image_ext%\"==\".jpeg\" set jpg=1\r\n" +
                 ")\r\n" +
                 "if \"" + param_mode.ToString() + "\"==\"auto_scale\" if \"%jpg%\"==\"1\" set \"Mode=noise_scale " + param_denoise.ToString() + "\"\r\n" +
                 "if \"" + param_mode.ToString() + "\"==\"auto_scale\" if not \"%jpg%\"==\"1\" set \"Mode=scale\"\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"auto_scale\" set \"Mode=" + param_mode.ToString() + " " + param_denoise.ToString() + "\"\r\n" +
                 "set \"Temporary_Name=" + random32.ToString() + "_%RANDOM%_%RANDOM%_%RANDOM%_\"\r\n" +
                 // アルファチャンネルが無い場合は普通に拡大
                 "if not \"%image_alpha%\"==\"true\" " + waifu2xbinary.ToString() + " " + "-i" + " " + "%Image_path%" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_penultimate.png\"\r\n" +
                 // 元のファイル名がユニコードでも処理出来るようにテンポフォルダに別名でコピーする
                 "if not \"%image_alpha%\"==\"true\" if not \"%ERRORLEVEL%\"==\"0\" copy /Y %Image_path% \"%TEMP%\\%Temporary_Name%%Image_ext%\" >nul&& " + waifu2xbinary.ToString() + " " + "-i" + " " + "\"%TEMP%\\%Temporary_Name%%Image_ext%\"" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_penultimate.png\"\r\n" +
                 // アルファチャンネルが有ったらImageMagickで分離して拡大
                 "if \"%image_alpha%\"==\"true\" (\r\n" +
                 "   convert.exe %Image_path% -channel RGBA -separate \"%TEMP%\\%Temporary_Name%.png\"\r\n" +
                 "   for /f \"delims=\" %%a in ('identify.exe -format \"%%k\" \"%TEMP%\\%Temporary_Name%-3.png\"') do set \"image_alpha_color=%%a\"\r\n" +
                 "   convert.exe \"%TEMP%\\%Temporary_Name%-0.png\" \"%TEMP%\\%Temporary_Name%-1.png\" \"%TEMP%\\%Temporary_Name%-2.png\" -channel RGB -combine \"%TEMP%\\%Temporary_Name%_RGB.png\"\r\n" +
                 "   " + waifu2xbinary.ToString() + " " + " -i" + " " + "\"%TEMP%\\%Temporary_Name%_RGB.png\"" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"\r\n" +
                 ")\r\n" +
                 "if \"%image_alpha_color%\"==\"1\" for /f \"delims=\" %%a in ('identify.exe -format \"%%w\" \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"') do set \"image_2x_width=%%a\"\r\n" +
                 "if \"%image_alpha_color%\"==\"1\" for /f \"delims=\" %%a in ('identify.exe -format \"%%h\" \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"') do set \"image_2x_height=%%a\"\r\n" +
                 "if \"%image_alpha%\"==\"true\" (\r\n" +
                 "   if \"%image_alpha_color%\"==\"1\" convert.exe \"%TEMP%\\%Temporary_Name%-3.png\" -sample %image_2x_width%x%image_2x_height%! \"%TEMP%\\%Temporary_Name%_alpha_2x.png\"\r\n" +
                 "   if not \"%image_alpha_color%\"==\"1\" " + waifu2xbinary.ToString() + " " + "-i" + " " + "\"%TEMP%\\%Temporary_Name%-3.png\"" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_alpha_2x.png\"\r\n" +
                 "   convert.exe \"%TEMP%\\%Temporary_Name%_RGB_2x.png\" -channel RGB -separate \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"\r\n" +
                 "   convert.exe \"%TEMP%\\%Temporary_Name%_RGB_2x-0.png\" \"%TEMP%\\%Temporary_Name%_RGB_2x-1.png\" \"%TEMP%\\%Temporary_Name%_RGB_2x-2.png\" \"%TEMP%\\%Temporary_Name%_alpha_2x.png\" -channel RGBA -combine \"%TEMP%\\%Temporary_Name%_penultimate.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-0.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-1.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-2.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-3.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x-0.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x-1.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x-2.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_alpha_2x.png\"\r\n" +
                 ")\r\n" +
                 "if not \"%output_width%\"==\"\" if not \"%output_height%\"==\"\" convert.exe \"%TEMP%\\%Temporary_Name%_penultimate.png\" -resize %output_width%x%output_height%" + Not_Aspect_ratio_keep_argument.ToString() + " \"%TEMP%\\%Temporary_Name%_penultimate2.png\" >NUL && move /Y \"%TEMP%\\%Temporary_Name%_penultimate2.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if not \"%output_width%\"==\"\" if \"%output_height%\"==\"\" convert.exe \"%TEMP%\\%Temporary_Name%_penultimate.png\" -resize %output_width%x \"%TEMP%\\%Temporary_Name%_penultimate2.png\" >NUL && move /Y \"%TEMP%\\%Temporary_Name%_penultimate2.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if \"%output_width%\"==\"\" if not \"%output_height%\"==\"\" convert.exe \"%TEMP%\\%Temporary_Name%_penultimate.png\" -resize x%output_height% \"%TEMP%\\%Temporary_Name%_penultimate2.png\" >NUL && move /Y \"%TEMP%\\%Temporary_Name%_penultimate2.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if \"%output_width%\"==\"\" if \"%output_height%\"==\"\" move /Y \"%TEMP%\\%Temporary_Name%_penultimate.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if exist \"%TEMP%\\%Temporary_Name%%Image_ext%\" del \"%TEMP%\\%Temporary_Name%%Image_ext%\" >nul\"\r\n" +
                 "if exist \"%TEMP%\\%Temporary_Name%_penultimate.png\" del \"%TEMP%\\%Temporary_Name%_penultimate.png\" >nul\"\r\n" +
                 
                 
                 "endlocal\r\n" +
                 "set Image_path=\r\n" +
                 "del \"" + waifu2x_bat.ToString() + "\"\r\n" +
                 "exit /b\r\n" +
                 "\"\r\n" +
                 ":scale_ratio_set\r\n" +
                 "if not \"%keep_aspect_ratio%\"==\"1\" (\r\n" +
                 "   call :scale_ratio_set_width\r\n" +
                 "   call :scale_ratio_set_height\r\n" +
                 "   exit /b\r\n" +
                 ")\r\n" +
                 "if \"%image_width%%image_height%\"==\"\" exit /b\r\n" +
                 "set /a image_height_nx=%image_height%*%scale_ratio%\r\n" +
                 "set /a image_width_nx=%image_width%*%scale_ratio%\r\n" +
                 "if %output_height% LEQ %image_height_nx% exit /b\r\n" +
                 "if %output_width% LEQ %image_width_nx% exit /b\r\n" +
                 "set /a scale_ratio=%scale_ratio%*2\r\n" +
                 "goto scale_ratio_set\r\n" +
                 "exit /b\r\n" +
                 "\r\n" +
                 ":scale_ratio_set_width\r\n" +
                 "if \"%output_width%\"==\"\" exit /b\r\n" +
                 "set /a image_width_nx=%image_width%*%scale_ratio%\r\n" +
                 "if not %output_width% LEQ %image_width_nx% (\r\n" +
                 "   set /a scale_ratio=%scale_ratio%*2\r\n" +
                 "   goto scale_ratio_set_width\r\n" +
                 ")\r\n" +
                 "exit /b\r\n" +
                 "\r\n" +
                 ":scale_ratio_set_height\r\n" +
                 "if \"%output_height%\"==\"\" exit /b\r\n" +
                 "set /a image_height_nx=%image_height%*%scale_ratio%\r\n" +
                 "if not %output_height% LEQ %image_height_nx% (\r\n" +
                 "   set /a scale_ratio=%scale_ratio%*2\r\n" +
                 "   goto scale_ratio_set_height\r\n" +
                 ")\r\n" +
                 "exit /b\r\n" 
            ;
                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                @waifu2x_bat.ToString(),
                false
                // ,
                // System.Text.Encoding.GetEncoding("utf-8")
                );
                sw.Write(TextBox1);
                sw.Close();

                psinfo.FileName = waifu2x_bat.ToString();

                // psinfo.Arguments = full_param;
                    psinfo.RedirectStandardError = true;
                    psinfo.RedirectStandardOutput = true;
                    psinfo.UseShellExecute = false;
                    psinfo.WorkingDirectory = App.directory;
                    psinfo.CreateNoWindow = true;
                    psinfo.WindowStyle = ProcessWindowStyle.Hidden;
                    pHandle.StartInfo = psinfo;
                    pHandle.EnableRaisingEvents = true;
                    pHandle.OutputDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
                    //pHandle.ErrorDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
                    pHandle.Exited += new EventHandler(OnProcessExit);

                // Starts working
                    console_buffer.Clear();


            }
            if (DandD_Mode == false) if (Directory.Exists(this.txtSrcPath.Text))
            {

                
                // Assemble parameters

                    //param_tta.ToString()
                    // Setup ProcessStartInfo
                       string TextBox1 = "@echo off\r\n" +
                       "chcp 65001 >nul\r\n" +
                        "set \"OutputFolder=" + param_dst.ToString() + "\"\r\n" +
                        //"set OutputFolder=%OutputFolder:\"=%\r\n" +
                        "for %%A IN (\"" + param_src.ToString() + "\") do set \"A=%%~aA\"\r\n" +
                        "IF not \"%A:~0,1%\"==\"d\" goto end\r\n" +
                        "if \"" + param_informat.ToString() +"\"==\"\" goto end\r\n" +
                        "set \"str=" + param_src.ToString() + "\"\r\n" +
                        "set \"len=0\"\r\n" +
                        "call :word_count\r\n" +
                        "if %len% neq 3 set /a len+=1\r\n" +
                        "set \"FileCount=0\"\r\n" +
                        "set \"ProcessedCount=0\"\r\n" +
                        "pushd \"" + param_src.ToString() + "\"\r\n" +
                        "FOR /f \"DELIMS=\" %%A IN ('dir " + param_informat.ToString() + " /A-D /S /B ^| find /c /v \"\"') DO SET \"FileCount=%%A\"\r\n" +
                        "popd\r\n" +
                        "for /r \"" + param_src.ToString() + "\" %%i in (" + param_informat.ToString() + ") do set Image_path=\"%%i\"&&call :waifu2x_run\r\n" +
                        "\r\n" +
                        "cls\r\n" +
                        "goto end\r\n" +
                        "\r\n" +
                        ":word_count\r\n" +
                        "if not \"%str%\"==\"\" (\r\n" +
                        "    set \"str=%str:~1%\"\r\n" +
                        "    set /a len=%len%+1\r\n" +
                        "    goto :word_count\r\n" +
                        ")\r\n" +
                        "exit /b\r\n" +
                        "\r\n" +
                        ":waifu2x_run\r\n" +
                        "echo progress %ProcessedCount%/%FileCount%\r\n" +
                        // "cls\r\n" +
                        "setlocal\r\n" +
                        "FOR %%A IN (%Image_path%) DO set \"relative_path=%%~dpA\"\r\n" +
                        "FOR %%A IN (%Image_path%) DO set \"OUTPUT_Name=%%~nA\"\r\n" +
                        "call set \"relative_path=%%relative_path:~%len%%%\"\r\n" +
                        "set \"Output_dir=%OutputFolder%%relative_path%\"\r\n" +
                        "if not exist \"%Output_dir%\" mkdir \"%Output_dir%\"\r\n" +
                        "if exist \"%Output_dir%%OUTPUT_Name%.png\" goto waifu2x_run_skip\r\n" +


                 // bat共通の処理
                 "set \"keep_aspect_ratio=" + Aspect_ratio_keep_argument.ToString() + "\"\r\n" + 
                 "set \"scale_ratio=" + this.slider_zoom.Value.ToString() + "\"\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"noise\" set \"output_width=" + this.output_width.Text + "\"\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"noise\" set \"output_height=" + this.output_height.Text + "\"\r\n" +
                 "FOR %%A IN (%Image_path%) DO set \"Image_ext=%%~xA\"\r\n" +
                 "if /i \"%Image_ext%\"==\".png\" identify.exe -format \"%%A\" %Image_path% | find \"Blend\"> NUL && set image_alpha=true\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"noise\" if not \"%output_width%%output_height%\"==\"\" (\r\n" +
                 "   for /f \"delims=\" %%a in ('identify.exe -format \"%%w\" %Image_path%') do set \"image_width=%%a\"\r\n" +
                 "   for /f \"delims=\" %%a in ('identify.exe -format \"%%h\" %Image_path%') do set \"image_height=%%a\"\r\n" +
                 "   set scale_ratio=1\r\n" +
                 "   call :scale_ratio_set\r\n" +
                 ")\r\n" +
                 "if \"" + param_mode.ToString() + "\"==\"auto_scale\" (\r\n" +
                 "   if /i \"%Image_ext%\"==\".jpg\" set jpg=1\r\n" +
                 "   if /i \"%Image_ext%\"==\".jpeg\" set jpg=1\r\n" +
                 ")\r\n" +
                 "if \"" + param_mode.ToString() + "\"==\"auto_scale\" if \"%jpg%\"==\"1\" set \"Mode=noise_scale " + param_denoise.ToString() + "\"\r\n" +
                 "if \"" + param_mode.ToString() + "\"==\"auto_scale\" if not \"%jpg%\"==\"1\" set \"Mode=scale\"\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"auto_scale\" set \"Mode=" + param_mode.ToString() + " " + param_denoise.ToString() + "\"\r\n" +
                 "set \"Temporary_Name=" + random32.ToString() + "_%RANDOM%_%RANDOM%_%RANDOM%_\"\r\n" +
                 // アルファチャンネルが無い場合は普通に拡大
                 "if not \"%image_alpha%\"==\"true\" " + waifu2xbinary.ToString() + " " + "-i" + " " + "%Image_path%" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_penultimate.png\" >nul\r\n" +
                 // 元のファイル名がユニコードでも処理出来るようにテンポフォルダに別名でコピーする
                 "if not \"%image_alpha%\"==\"true\" if not \"%ERRORLEVEL%\"==\"0\" copy /Y %Image_path% \"%TEMP%\\%Temporary_Name%%Image_ext%\" >nul&& " + waifu2xbinary.ToString() + " " + "-i" + " " + "\"%TEMP%\\%Temporary_Name%%Image_ext%\"" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_penultimate.png\" >nul\r\n" +
                 // アルファチャンネルが有ったらImageMagickで分離して拡大
                 "if \"%image_alpha%\"==\"true\" (\r\n" +
                 "   convert.exe %Image_path% -channel RGBA -separate \"%TEMP%\\%Temporary_Name%.png\"\r\n" +
                 "   for /f \"delims=\" %%a in ('identify.exe -format \"%%k\" \"%TEMP%\\%Temporary_Name%-3.png\"') do set \"image_alpha_color=%%a\"\r\n" +
                 "   convert.exe \"%TEMP%\\%Temporary_Name%-0.png\" \"%TEMP%\\%Temporary_Name%-1.png\" \"%TEMP%\\%Temporary_Name%-2.png\" -channel RGB -combine \"%TEMP%\\%Temporary_Name%_RGB.png\"\r\n" +
                 "   " + waifu2xbinary.ToString() + " " + " -i" + " " + "\"%TEMP%\\%Temporary_Name%_RGB.png\"" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"\r\n" +
                 ") >nul\r\n" +
                 "if \"%image_alpha_color%\"==\"1\" for /f \"delims=\" %%a in ('identify.exe -format \"%%w\" \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"') do set \"image_2x_width=%%a\"\r\n" +
                 "if \"%image_alpha_color%\"==\"1\" for /f \"delims=\" %%a in ('identify.exe -format \"%%h\" \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"') do set \"image_2x_height=%%a\"\r\n" +
                 "if \"%image_alpha%\"==\"true\" (\r\n" +
                 "   if \"%image_alpha_color%\"==\"1\" convert.exe \"%TEMP%\\%Temporary_Name%-3.png\" -sample %image_2x_width%x%image_2x_height%! \"%TEMP%\\%Temporary_Name%_alpha_2x.png\"\r\n" +
                 "   if not \"%image_alpha_color%\"==\"1\" " + waifu2xbinary.ToString() + " " + "-i" + " " + "\"%TEMP%\\%Temporary_Name%-3.png\"" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_alpha_2x.png\"\r\n" +
                 "   convert.exe \"%TEMP%\\%Temporary_Name%_RGB_2x.png\" -channel RGB -separate \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"\r\n" +
                 "   convert.exe \"%TEMP%\\%Temporary_Name%_RGB_2x-0.png\" \"%TEMP%\\%Temporary_Name%_RGB_2x-1.png\" \"%TEMP%\\%Temporary_Name%_RGB_2x-2.png\" \"%TEMP%\\%Temporary_Name%_alpha_2x.png\" -channel RGBA -combine \"%TEMP%\\%Temporary_Name%_penultimate.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-0.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-1.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-2.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-3.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x-0.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x-1.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x-2.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_alpha_2x.png\"\r\n" +
                 ") >nul\r\n" +
                 "if not \"%output_width%\"==\"\" if not \"%output_height%\"==\"\" convert.exe \"%TEMP%\\%Temporary_Name%_penultimate.png\" -resize %output_width%x%output_height%" + Not_Aspect_ratio_keep_argument.ToString() + " \"%TEMP%\\%Temporary_Name%_penultimate2.png\" >NUL && move /Y \"%TEMP%\\%Temporary_Name%_penultimate2.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if not \"%output_width%\"==\"\" if \"%output_height%\"==\"\" convert.exe \"%TEMP%\\%Temporary_Name%_penultimate.png\" -resize %output_width%x \"%TEMP%\\%Temporary_Name%_penultimate2.png\" >NUL && move /Y \"%TEMP%\\%Temporary_Name%_penultimate2.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if \"%output_width%\"==\"\" if not \"%output_height%\"==\"\" convert.exe \"%TEMP%\\%Temporary_Name%_penultimate.png\" -resize x%output_height% \"%TEMP%\\%Temporary_Name%_penultimate2.png\" >NUL && move /Y \"%TEMP%\\%Temporary_Name%_penultimate2.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if \"%output_width%\"==\"\" if \"%output_height%\"==\"\" move /Y \"%TEMP%\\%Temporary_Name%_penultimate.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if exist \"%TEMP%\\%Temporary_Name%%Image_ext%\" del \"%TEMP%\\%Temporary_Name%%Image_ext%\" >nul\"\r\n" +
                 "if exist \"%TEMP%\\%Temporary_Name%_penultimate.png\" del \"%TEMP%\\%Temporary_Name%_penultimate.png\" >nul\"\r\n" +
                        
                        
                        
                        ":waifu2x_run_skip\r\n" +
                        "endlocal\r\n" +
                        "set Image_path=\r\n" +
                        "set /a ProcessedCount=%ProcessedCount%+1\r\n" +
                        "exit /b\r\n" +
                        "\r\n" +
                        ":scale_ratio_set\r\n" +
                        "if not \"%keep_aspect_ratio%\"==\"1\" (\r\n" +
                        "   call :scale_ratio_set_width\r\n" +
                        "   call :scale_ratio_set_height\r\n" +
                        "   exit /b\r\n" +
                        ")\r\n" +
                        "if \"%image_width%%image_height%\"==\"\" exit /b\r\n" +
                        "set /a image_height_nx=%image_height%*%scale_ratio%\r\n" +
                        "set /a image_width_nx=%image_width%*%scale_ratio%\r\n" +
                        "if %output_height% LEQ %image_height_nx% exit /b\r\n" +
                        "if %output_width% LEQ %image_width_nx% exit /b\r\n" +
                        "set /a scale_ratio=%scale_ratio%*2\r\n" +
                        "goto scale_ratio_set\r\n" +
                        "exit /b\r\n" +
                        "\r\n" +
                        ":scale_ratio_set_width\r\n" +
                        "if \"%output_width%\"==\"\" exit /b\r\n" +
                        "set /a image_width_nx=%image_width%*%scale_ratio%\r\n" +
                        "if not %output_width% LEQ %image_width_nx% (\r\n" +
                        "   set /a scale_ratio=%scale_ratio%*2\r\n" +
                        "   goto scale_ratio_set_width\r\n" +
                        ")\r\n" +
                        "exit /b\r\n" +
                        "\r\n" +
                        ":scale_ratio_set_height\r\n" +
                        "if \"%output_height%\"==\"\" exit /b\r\n" +
                        "set /a image_height_nx=%image_height%*%scale_ratio%\r\n" +
                        "if not %output_height% LEQ %image_height_nx% (\r\n" +
                        "   set /a scale_ratio=%scale_ratio%*2\r\n" +
                        "   goto scale_ratio_set_height\r\n" +
                        ")\r\n" +
                        "exit /b\r\n" +
                        "\r\n" +
                        ":end\r\n" +
                        "del \"" + waifu2x_bat.ToString() + "\"\r\n" +
                        "exit /b\r\n"
                   ;
                   System.IO.StreamWriter sw = new System.IO.StreamWriter(
                   @waifu2x_bat.ToString(),
                   false
                   // ,
                   // System.Text.Encoding.GetEncoding("utf-8")
                   );
                   sw.Write(TextBox1);
                   sw.Close();

                    psinfo.FileName = waifu2x_bat.ToString();
                    //psinfo.Arguments = full_param;
                    psinfo.RedirectStandardError = true;
                    psinfo.RedirectStandardOutput = true;
                    psinfo.UseShellExecute = false;
                    psinfo.WorkingDirectory = App.directory;
                    psinfo.CreateNoWindow = true;
                    psinfo.WindowStyle = ProcessWindowStyle.Hidden;
                    pHandle.StartInfo = psinfo;
                    pHandle.EnableRaisingEvents = true;
                    pHandle.OutputDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
                    //pHandle.ErrorDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
                    pHandle.Exited += new EventHandler(OnProcessExit);

                    console_buffer.Clear();
                    //console_buffer.Append(full_param);
                    //console_buffer.Append("\n");

            }
            if (DandD_Mode == true)
            { 
                // Assemble parameters

                //param_tta.ToString()
                // Setup ProcessStartInfo
                string TextBox1 = "cls\r\n" + 
                 "goto end\r\n" +
                 "\r\n" +
                 ":list_Allocation\r\n" +
                 "set FileCount=" + FileCount + "\r\n" +
                 "set \"OutputFolder=" + param_dst_dd.ToString() + "\"\r\n" +
                 "echo progress %ProcessedCount%/%FileCount%\r\n" +
                 // "cls\r\n" +
                 "for %%A IN (%list_path%) do set \"A=%%~aA\"\r\n" +
                 "IF not \"%A:~0,1%\"==\"d\" set list_path_file=1\r\n" +
                 "IF \"%A:~0,1%\"==\"d\" set list_path_dir=1\r\n" +
                 // ファイルの処理
                 "if \"%list_path_file%\"==\"1\" set Image_path=%list_path%\r\n" +
                 "if \"%list_path_file%\"==\"1\" call :waifu2x_run\r\n" +
                 // フォルダの処理
                 "if \"%list_path_dir%\"==\"1\" if \"%OutputFolder%\"==\"\" for %%A in (%list_path%) do set \"str=%%~A\"\r\n" +
                 "if \"%list_path_dir%\"==\"1\" if not \"%OutputFolder%\"==\"\" for %%A in (%list_path%) do set \"str=%%~dpA\"\r\n" +
                 "if \"%list_path_dir%\"==\"1\" set \"len=0\"\r\n" +
                 "if \"%list_path_dir%\"==\"1\" call :word_count\r\n" +
                 "if \"%list_path_dir%\"==\"1\" for /r %list_path% %%i in (" + param_informat.ToString() + ") do (\r\n" +
                 "   set \"OUTPUT_Name=%%~ni\"\r\n" +
                 "   set Image_path=\"%%i\"\r\n" +
                 "   call :waifu2x_run\r\n" +
                 ")\r\n" +
                 "set /a ProcessedCount=%ProcessedCount%+1\r\n" +
                 "exit /b\r\n" +
                 ":word_count\r\n" +
                 "if not \"%str%\"==\"\" (\r\n" +
                 "    set \"str=%str:~1%\"\r\n" +
                 "    set /a len=%len%+1\r\n" +
                 "    goto :word_count\r\n" +
                 ")\r\n" +
                 "exit /b\r\n" +
                 "\r\n" +
                 ":waifu2x_run\r\n" +
                 "setlocal\r\n" +
                 // ファイルの処理
                 "if \"%list_path_file%\"==\"1\" for %%A IN (%list_path%) do set \"OUTPUT_Name=%%~nA" + param_dst.ToString() + "\"\r\n" +
                 "if \"%list_path_file%\"==\"1\" if \"%OutputFolder%\"==\"\" for %%A IN (%list_path%) DO set \"Output_dir=%%~dpA\"\r\n" +
                 "if \"%list_path_file%\"==\"1\" if not \"%OutputFolder%\"==\"\" set \"Output_dir=%OutputFolder%\"\r\n" +
                 //フォルダの処理
                 "if \"%list_path_dir%\"==\"1\" if \"%OutputFolder%\"==\"\" for %%A IN (%list_path%) DO set \"OutputFolder=%%~A" + param_dst.ToString() + "\\\"\r\n" +
                 "if \"%list_path_dir%\"==\"1\" FOR %%A IN (%Image_path%) DO set \"relative_path=%%~dpA\"\r\n" +
                 "if \"%list_path_dir%\"==\"1\" call set \"relative_path=%%relative_path:~%len%%%\"\r\n" +
                 "if \"%list_path_dir%\"==\"1\" set \"Output_dir=%OutputFolder%%relative_path%\"\r\n" +
                 "if not exist \"%Output_dir%\" mkdir \"%Output_dir%\"\r\n" +
                 "if exist \"%Output_dir%%OUTPUT_Name%.png\" goto waifu2x_run_skip\r\n" +


                 // bat共通の処理
                 "set \"keep_aspect_ratio=" + Aspect_ratio_keep_argument.ToString() + "\"\r\n" + 
                 "set \"scale_ratio=" + this.slider_zoom.Value.ToString() + "\"\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"noise\" set \"output_width=" + this.output_width.Text + "\"\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"noise\" set \"output_height=" + this.output_height.Text + "\"\r\n" +
                 "FOR %%A IN (%Image_path%) DO set \"Image_ext=%%~xA\"\r\n" +
                 "if /i \"%Image_ext%\"==\".png\" identify.exe -format \"%%A\" %Image_path% | find \"Blend\"> NUL && set image_alpha=true\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"noise\" if not \"%output_width%%output_height%\"==\"\" (\r\n" +
                 "   for /f \"delims=\" %%a in ('identify.exe -format \"%%w\" %Image_path%') do set \"image_width=%%a\"\r\n" +
                 "   for /f \"delims=\" %%a in ('identify.exe -format \"%%h\" %Image_path%') do set \"image_height=%%a\"\r\n" +
                 "   set scale_ratio=1\r\n" +
                 "   call :scale_ratio_set\r\n" +
                 ")\r\n" +
                 "if \"" + param_mode.ToString() + "\"==\"auto_scale\" (\r\n" +
                 "   if /i \"%Image_ext%\"==\".jpg\" set jpg=1\r\n" +
                 "   if /i \"%Image_ext%\"==\".jpeg\" set jpg=1\r\n" +
                 ")\r\n" +
                 "if \"" + param_mode.ToString() + "\"==\"auto_scale\" if \"%jpg%\"==\"1\" set \"Mode=noise_scale " + param_denoise.ToString() + "\"\r\n" +
                 "if \"" + param_mode.ToString() + "\"==\"auto_scale\" if not \"%jpg%\"==\"1\" set \"Mode=scale\"\r\n" +
                 "if not \"" + param_mode.ToString() + "\"==\"auto_scale\" set \"Mode=" + param_mode.ToString() + " " + param_denoise.ToString() + "\"\r\n" +
                 "set \"Temporary_Name=" + random32.ToString() + "_%RANDOM%_%RANDOM%_%RANDOM%_\"\r\n" +
                 // アルファチャンネルが無い場合は普通に拡大
                 "if not \"%image_alpha%\"==\"true\" " + waifu2xbinary.ToString() + " " + "-i" + " " + "%Image_path%" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_penultimate.png\" >nul\r\n" +
                 // 元のファイル名がユニコードでも処理出来るようにテンポフォルダに別名でコピーする
                 "if not \"%image_alpha%\"==\"true\" if not \"%ERRORLEVEL%\"==\"0\" copy /Y %Image_path% \"%TEMP%\\%Temporary_Name%%Image_ext%\" >nul&& " + waifu2xbinary.ToString() + " " + "-i" + " " + "\"%TEMP%\\%Temporary_Name%%Image_ext%\"" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_penultimate.png\" >nul\r\n" +
                 // アルファチャンネルが有ったらImageMagickで分離して拡大
                 "if \"%image_alpha%\"==\"true\" (\r\n" +
                 "   convert.exe %Image_path% -channel RGBA -separate \"%TEMP%\\%Temporary_Name%.png\"\r\n" +
                 "   for /f \"delims=\" %%a in ('identify.exe -format \"%%k\" \"%TEMP%\\%Temporary_Name%-3.png\"') do set \"image_alpha_color=%%a\"\r\n" +
                 "   convert.exe \"%TEMP%\\%Temporary_Name%-0.png\" \"%TEMP%\\%Temporary_Name%-1.png\" \"%TEMP%\\%Temporary_Name%-2.png\" -channel RGB -combine \"%TEMP%\\%Temporary_Name%_RGB.png\"\r\n" +
                 "   " + waifu2xbinary.ToString() + " " + " -i" + " " + "\"%TEMP%\\%Temporary_Name%_RGB.png\"" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"\r\n" +
                 ") >nul\r\n" +
                 "if \"%image_alpha_color%\"==\"1\" for /f \"delims=\" %%a in ('identify.exe -format \"%%w\" \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"') do set \"image_2x_width=%%a\"\r\n" +
                 "if \"%image_alpha_color%\"==\"1\" for /f \"delims=\" %%a in ('identify.exe -format \"%%h\" \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"') do set \"image_2x_height=%%a\"\r\n" +
                 "if \"%image_alpha%\"==\"true\" (\r\n" +
                 "   if \"%image_alpha_color%\"==\"1\" convert.exe \"%TEMP%\\%Temporary_Name%-3.png\" -sample %image_2x_width%x%image_2x_height%! \"%TEMP%\\%Temporary_Name%_alpha_2x.png\"\r\n" +
                 "   if not \"%image_alpha_color%\"==\"1\" " + waifu2xbinary.ToString() + " " + "-i" + " " + "\"%TEMP%\\%Temporary_Name%-3.png\"" + " " + "-m %mode%" + " " + param_mag.ToString() + " " + param_model.ToString() + " " + param_block.ToString() + " " + param_device.ToString() + " " + "-o \"%TEMP%\\%Temporary_Name%_alpha_2x.png\"\r\n" +
                 "   convert.exe \"%TEMP%\\%Temporary_Name%_RGB_2x.png\" -channel RGB -separate \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"\r\n" +
                 "   convert.exe \"%TEMP%\\%Temporary_Name%_RGB_2x-0.png\" \"%TEMP%\\%Temporary_Name%_RGB_2x-1.png\" \"%TEMP%\\%Temporary_Name%_RGB_2x-2.png\" \"%TEMP%\\%Temporary_Name%_alpha_2x.png\" -channel RGBA -combine \"%TEMP%\\%Temporary_Name%_penultimate.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-0.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-1.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-2.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%-3.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x-0.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x-1.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_RGB_2x-2.png\"\r\n" +
                 "   del \"%TEMP%\\%Temporary_Name%_alpha_2x.png\"\r\n" +
                 ") >nul\r\n" +
                 "if not \"%output_width%\"==\"\" if not \"%output_height%\"==\"\" convert.exe \"%TEMP%\\%Temporary_Name%_penultimate.png\" -resize %output_width%x%output_height%" + Not_Aspect_ratio_keep_argument.ToString() + " \"%TEMP%\\%Temporary_Name%_penultimate2.png\" >NUL && move /Y \"%TEMP%\\%Temporary_Name%_penultimate2.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if not \"%output_width%\"==\"\" if \"%output_height%\"==\"\" convert.exe \"%TEMP%\\%Temporary_Name%_penultimate.png\" -resize %output_width%x \"%TEMP%\\%Temporary_Name%_penultimate2.png\" >NUL && move /Y \"%TEMP%\\%Temporary_Name%_penultimate2.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if \"%output_width%\"==\"\" if not \"%output_height%\"==\"\" convert.exe \"%TEMP%\\%Temporary_Name%_penultimate.png\" -resize x%output_height% \"%TEMP%\\%Temporary_Name%_penultimate2.png\" >NUL && move /Y \"%TEMP%\\%Temporary_Name%_penultimate2.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if \"%output_width%\"==\"\" if \"%output_height%\"==\"\" move /Y \"%TEMP%\\%Temporary_Name%_penultimate.png\" \"%Output_dir%%OUTPUT_Name%.png\" >NUL\r\n" +
                 "if exist \"%TEMP%\\%Temporary_Name%%Image_ext%\" del \"%TEMP%\\%Temporary_Name%%Image_ext%\" >nul\"\r\n" +
                 "if exist \"%TEMP%\\%Temporary_Name%_penultimate.png\" del \"%TEMP%\\%Temporary_Name%_penultimate.png\" >nul\"\r\n" +
                 
                 
                 
                 
                 ":waifu2x_run_skip\r\n" +
                 "endlocal\r\n" +
                 "set Image_path=\r\n" +
                 "exit /b\r\n" +
                 "\r\n" +
                 ":scale_ratio_set\r\n" +
                 "if not \"%keep_aspect_ratio%\"==\"1\" (\r\n" +
                 "   call :scale_ratio_set_width\r\n" +
                 "   call :scale_ratio_set_height\r\n" +
                 "   exit /b\r\n" +
                 ")\r\n" +
                 "if \"%image_width%%image_height%\"==\"\" exit /b\r\n" +
                 "set /a image_height_nx=%image_height%*%scale_ratio%\r\n" +
                 "set /a image_width_nx=%image_width%*%scale_ratio%\r\n" +
                 "if %output_height% LEQ %image_height_nx% exit /b\r\n" +
                 "if %output_width% LEQ %image_width_nx% exit /b\r\n" +
                 "set /a scale_ratio=%scale_ratio%*2\r\n" +
                 "goto scale_ratio_set\r\n" +
                 "exit /b\r\n" +
                 "\r\n" +
                 ":scale_ratio_set_width\r\n" +
                 "if \"%output_width%\"==\"\" exit /b\r\n" +
                 "set /a image_width_nx=%image_width%*%scale_ratio%\r\n" +
                 "if not %output_width% LEQ %image_width_nx% (\r\n" +
                 "   set /a scale_ratio=%scale_ratio%*2\r\n" +
                 "   goto scale_ratio_set_width\r\n" +
                 ")\r\n" +
                 "exit /b\r\n" +
                 "\r\n" +
                 ":scale_ratio_set_height\r\n" +
                 "if \"%output_height%\"==\"\" exit /b\r\n" +
                 "set /a image_height_nx=%image_height%*%scale_ratio%\r\n" +
                 "if not %output_height% LEQ %image_height_nx% (\r\n" +
                 "   set /a scale_ratio=%scale_ratio%*2\r\n" +
                 "   goto scale_ratio_set_height\r\n" +
                 ")\r\n" +
                 "exit /b\r\n" +
                 "\r\n" +
                 ":end\r\n" +
                 "del \"" + waifu2x_bat.ToString() + "\"\r\n" +
                 "exit /b\r\n"
            ;
                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                @waifu2x_bat.ToString(),
                true
                // ,
                // System.Text.Encoding.GetEncoding("utf-8")
                );
                sw.Write(TextBox1);
                sw.Close();

                psinfo.FileName = waifu2x_bat.ToString();
                //psinfo.Arguments = full_param;
                psinfo.RedirectStandardError = true;
                psinfo.RedirectStandardOutput = true;
                psinfo.UseShellExecute = false;
                psinfo.WorkingDirectory = App.directory;
                psinfo.CreateNoWindow = true;
                psinfo.WindowStyle = ProcessWindowStyle.Hidden;
                pHandle.StartInfo = psinfo;
                pHandle.EnableRaisingEvents = true;
                pHandle.OutputDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
                //pHandle.ErrorDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
                pHandle.Exited += new EventHandler(OnProcessExit);

                console_buffer.Clear();
                //console_buffer.Append(full_param);
                //console_buffer.Append("\n");
            }
            try
            {
                //MessageBox.Show(full_param);
                bool pState = pHandle.Start();

            }
            catch (Exception)
            {
                pHandle.Kill();
                MessageBox.Show("Some parameters do not mix well and crashed...");
                //throw;
            }
            pHandle.BeginOutputReadLine();
            //pHandle.BeginErrorReadLine();
            //MessageBox.Show("Some parameters do not mix well and crashed...");

            //pHandle.WaitForExit();
            /*
            pHandle.CancelOutputRead();
            pHandle.Close();
            this.btnAbort.IsEnabled = false;
            this.btnRun.IsEnabled = true;
            this.CLIOutput.Text = console_buffer.ToString();
            */

        }
    }
}
