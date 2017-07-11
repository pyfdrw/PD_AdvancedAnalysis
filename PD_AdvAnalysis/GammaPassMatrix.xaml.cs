﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.CA.Scripting;
using VMS.DV.PD.Scripting;

namespace PD_AdvAnalysis
{
    /// <summary>
    /// Interaction logic for GammaPassMatrix.xaml
    /// </summary>

    public partial class GammaPassMatrix : UserControl
    {
        ComboBox meas_ddl = System.Windows.Application.Current.MainWindow.FindName("meas_ddl") as ComboBox;
        
        public Patient newcontext;//save these guys for use later.
        //VMS.DV.PD.Scripting.Application PDapp;// = System.Windows.Application.Current.MainWindow.FindName("PDApp") as VMS.DV.PD.Scripting.Application;
        PDPlanSetup plan;
        Course course;
        public PDBeam field;
        PortalDoseImage fieldm;
        PortalDoseImage fieldc;
        public GammaPassMatrix()
        {
            InitializeComponent();

        }
        private void calc_btn_Click(object sender, RoutedEventArgs e)
        {
            //newcontext = System.Windows.Application.Current.MainWindow.FindName("newcontext") as Patient;
            newcontext = PD_AdvAnalysis.MainWindow.newcontext;
            field = PD_AdvAnalysis.MainWindow.field;
            gamma_grd.Children.Clear();
            //here's where all the magic happens!!
            //check the fields are selected
            if (meas_ddl.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a field");
            }
            else if (String.IsNullOrEmpty(startdd_txt.Text) || string.IsNullOrEmpty(startdta_txt.Text) || string.IsNullOrEmpty(enddd_txt.Text) || string.IsNullOrEmpty(enddta_txt.Text) || string.IsNullOrEmpty(deldd_txt.Text) || string.IsNullOrEmpty(deldta_txt.Text) || string.IsNullOrEmpty(tol_txt.Text))
            {
                MessageBox.Show("Please input all numeric parameters in the appropriate box");
            }
            else
            {
                //get beams
                fieldm = field.PortalDoseImages.Where(i => i.Id == meas_ddl.SelectedItem.ToString()).First();
                //don't need to do the where clause for the predicted.
                double startdd = Convert.ToDouble(startdd_txt.Text) / 100; double endd = Convert.ToDouble(enddd_txt.Text) / 100; double deldd = Convert.ToDouble(deldd_txt.Text) / 100;
                double startdta = Convert.ToDouble(startdta_txt.Text); double enddta = Convert.ToDouble(enddta_txt.Text); double deldta = Convert.ToDouble(deldta_txt.Text);
                double tol = Convert.ToDouble(tol_txt.Text) / 100;
                double parm;
                Double.TryParse(testparam_txt.Text, out parm);
                parm = parm / 100;
                //setup portal dosimetry analysis template.
                IEnumerable<EvaluationTestDesc> tested = new List<EvaluationTestDesc> { new EvaluationTestDesc((EvaluationTestKind)EvalTestKind_cmb.SelectedIndex, parm, tol, false) };


                //new EvaluationTestDesc()
                //loop through all testable queries
                //columns go here.;
                //lay down the dta and dd labels.
                int bw = 50; int bh = 25;
                int marginx = bw; int marginy = bh;
                for (double i = startdd; i <= endd; i += deldd)
                {
                    TextBox dbox = new TextBox(); dbox.IsReadOnly = true; dbox.Text = String.Format("{0}%", i * 100);
                    dbox.Width = bw; dbox.Background = Brushes.White; dbox.BorderBrush = Brushes.Black;
                    dbox.HorizontalAlignment = HorizontalAlignment.Left; dbox.VerticalAlignment = VerticalAlignment.Top;
                    dbox.Height = bh;
                    dbox.Margin = new Thickness(marginx, 0, 0, 0);
                    gamma_grd.Children.Add(dbox);
                    marginx += bw;
                }
                for (double j = startdta; j <= enddta; j += deldta)
                {
                    TextBox dbox = new TextBox(); dbox.IsReadOnly = true; dbox.Text = String.Format("{0}mm", j);
                    dbox.Width = bw; dbox.Background = Brushes.White; dbox.BorderBrush = Brushes.Black;
                    dbox.HorizontalAlignment = HorizontalAlignment.Left; dbox.VerticalAlignment = VerticalAlignment.Top;
                    dbox.Height = bh;
                    dbox.Margin = new Thickness(0, marginy, 0, 0);
                    marginy += bh;
                    gamma_grd.Children.Add(dbox);

                }
                marginx = bw; marginy = 0;


                for (double i = startdta; i <= enddta; i += deldta)
                {
                    //lay down a new header for the column.
                    marginy += bh;
                    marginx = 0;
                    //rows go here
                    double margins_txt;
                    Double.TryParse(marg_txt.Text, out margins_txt);
                    double th_txt;
                    double.TryParse(threshold_txt.Text, out th_txt);
                    // MessageBox.Show(margins_txt.ToString());

                    int anal_mode = (bool)abs_rdb.IsChecked ? 0 : 2;
                    for (double j = startdd; j <= endd; j += deldd)
                    {
                        marginx += bw;
                        //lay down an initial grid for the dta and dd
                        TextBox header_box = new TextBox();
                        //modify the template
                        //PDTemplate template = new PDTemplate(false, false, false, AnalysisMode.CU, NormalizationMethod.Unknown, false, 0, (ROIType)ROITypes_cmb.SelectedIndex, margins_txt, j, i, false, tested);
                        PDTemplate template1 = new PDTemplate(false, false, false, (AnalysisMode)anal_mode, (NormalizationMethod)Normalizaton_cmb.SelectedIndex, (bool)threshold_chk.IsChecked, th_txt, (ROIType)ROITypes_cmb.SelectedIndex, margins_txt, j, i, false, tested);
                        //PDTemplate template2 = new PDTemplate(false,false, false, (AnalysisMode))
                        // PDTemplate template_test = new PDTemplate(false, false, false, AnalysisMode.CU, NormalizationMethod.Unknown, false, Convert.ToDouble(margins_txt.Text));
                        //apply the template to the analysis.

                        PDAnalysis analysis = fieldm.CreateTransientAnalysis(template1, field.PredictedDoseImage);
                        //get the gamma value. 
                        //ImageRT gamma = analysis.GammaImage;
                        //VMS.CA.Scripting.Frame F = gamma.FramesRT.First();
                        //ushort[,] pixels = new ushort[F.XSize, F.YSize];
                        //MessageBox.Show(analysis.GammaParamHistogramCutoff.ToString());//this is not the gamma pass rate.
                        //F.GetVoxels(0, pixels);
                        //double gamma_pass = GetGammaPassRate(gamma);
                        double gamma_pass = analysis.EvaluationTests.First().TestValue * 100;
                        //MessageBox.Show(gamma_pass.ToString());
                        TextBox gamma_box = new TextBox(); gamma_box.IsReadOnly = true; gamma_box.Text = gamma_pass.ToString("F3");
                        gamma_box.Width = bw; gamma_box.Height = bh; gamma_box.Background = gamma_pass < tol * 100 ? Brushes.Pink : Brushes.LightGreen; gamma_box.BorderBrush = Brushes.Black;
                        gamma_box.HorizontalAlignment = HorizontalAlignment.Left; gamma_box.VerticalAlignment = VerticalAlignment.Top;
                        gamma_box.Margin = new Thickness(marginx, marginy, 0, 0);
                        gamma_grd.Children.Add(gamma_box);
                    }
                }
            }
        }
        /*private double GetGammaPassRate(ImageRT image)
        {
            int count_pass = 0;
            int count_all = 0;
            //get first frame.
            VMS.CA.Scripting.Frame F = image.FramesRT.First();
            ushort[,] pixels = new ushort[F.XSize, F.YSize];
            F.GetVoxels(0, pixels);
            for (int i = 0; i < F.XSize; i++)
            {
                for (int j = 0; j < F.YSize; j++)
                {
                    if (F.VoxelToDisplayValue(pixels[i, j]) < 1.0)
                    {
                        count_pass++;
                    }
                    count_all++;
                }
            }
            double pass_rate = (Convert.ToDouble(count_pass) / Convert.ToDouble(count_all)) * 100;
            return (pass_rate);
        }*/

        private void EvalTestKind_cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string[] values = new string[]{"0, 1, 2, 3, 6, 7, 10, 11" };
            int[] sel_in = new int[] { 0, 1, 2, 3, 6, 7, 10, 11 };

            if (EvalTestKind_cmb.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a gamma analysis parameter");
            }

            else if (sel_in.Contains(EvalTestKind_cmb.SelectedIndex))
            {
                testparam_txt.IsEnabled = true;
            }
            else
            {
                testparam_txt.IsEnabled = false;
            }
        }
    }
}