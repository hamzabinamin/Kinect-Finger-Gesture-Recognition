
namespace DTWGestureRecognition
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Linq;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.FaceTracking;
    using System.ComponentModel;
    using System.Threading;
    using HandDetection;
    using DWTGestureRecognition;
    using System.Speech.Synthesis;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Private State
        private const int eachLabelRecordingNb = 1;
        private const int Ignore = 2;
        private const int BufferSize = 32;
        private const int MinimumFrames = 6;
        private const int CaptureCountdownSeconds = 3;
        private string GestureSaveFileLocation = Environment.CurrentDirectory;
        private const string GestureBodySaveFileNamePrefix = @"BodyGestures";
        private const string GestureFaceSaveFileNamePrefix = @"FaceGestures";
        private const string GestureFaceOrientationSaveFileNamePrefix = @"FaceOrientationGestures";
        private const string GestureHandSaveFileNamePrefix = @"HandGestures";
        private const string GestureStaticHandSaveFileNamePrefix = @"StaticHandGesture";
        private const string GestureStaticBodySaveFileNamePrefix = @"StaticBodyGestures";
        private const string GestureStaticFaceSaveFileNamePrefix = @"StaticFaceGestures";
        private string[] staticFaceGestures = { "Neutral", "Yawn", "Sad", "Happy", "ReiseEyeBrows" };
        private string[] staticFaceAttributes = { "BrowLower", "BrowRaiser", "JawLower", "LipCornerDepressor", "LipRaiser", "LipStretcher" };
        private string[] staticBodyAttributes = { "HandLeft_X", "HandLeft_Y", "HandLeft_Z", "WristLeft_X", "WristLeft_Y", "WristLeft_Z", "ElbowLeft_X", "ElbowLeft_Y", "ElbowLeft_Z", 
                                                     "ElbowRight_X", "ElbowRight_Y", "ElbowRight_Z", "WristRight_X", "WristRight_Y", "WristRight_Z", "HandRight_X", "HandRight_Y", "HandRight_Z"};
        private string[] staticBodyGestures = { "@Left_Hand_Up", "@Left_Hand_To_The_Left", "@Left_Hand_To_The_Right", "@Right_Hand_Up", "@Right_Hand_To_The_Left", "@Right_Hand_To_The_Right", "@Both_Hands_Up", "@Both_Hands_Down" };
        private bool staticFaceInit = false;
        private bool staticBodyInit = false;
        private Weka.BayesNaive acFace;
        private Weka.BayesNaive acBody;
        private bool _capturing;
        private DtwGestureRecognizer _dtw1;
        private DtwGestureRecognizer _dtw2;
        private DtwGestureRecognizer _dtw3;
        private DtwGestureRecognizer _dtw4;
        private StaticGestureDataExtract staticFaceExtractor;
        private StaticGestureDataExtract staticBodyExtractor;
        private int _flipFlop;
        private ArrayList _bodyVideo;
        private ArrayList _recordedVideo;
        private ArrayList _handVideo;
        private double[] auxBodyVideo = { };
        private double[] auxFaceAnimationUnit = { };
        private ArrayList _faceVideo;
        private ArrayList _faceOrientationVideo;

        private DateTime _captureCountdown = DateTime.Now;
        private System.Windows.Forms.Timer _captureCountdownTimer;
        private KinectSensor _Kinect;
        public static Skeleton[] _FrameSkeletons;
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        private MouseControl mouse;
        FHandTracker fHandTracker;
        double[] storeCoords;
        double[] storeCoords2;
        SpeechSynthesizer reader = new SpeechSynthesizer();


        public static EnumIndexableCollection<FeaturePoint, PointF> facePointsS;
        EnumIndexableCollection<FeaturePoint, Vector3DF> facePoints3D;
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        #endregion Private State
        #region ctor + Window Events
        public MainWindow()
        {
            InitializeComponent();

            var faceTrackingViewerBinding = new System.Windows.Data.Binding("Kinect") { Source = sensorChooser };
            faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);
            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;

            mouse = new MouseControl();
            sensorChooser.Start();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {

            DiscoverKinectSensor(kinectChangedEventArgs);
            _dtw2 = new DtwGestureRecognizer(6, 0.45, 8, 3, 16);//face gesture dtw
            _dtw3 = new DtwGestureRecognizer(3, 0.45, 8, 3, 16);//face orientation gesture dtw
            _dtw1 = new DtwGestureRecognizer(18, 0.9, 3, 3, 10);//body movements dtw
            _dtw4 = new DtwGestureRecognizer(15, 0.3, 0.3, 1, 3);//hand dtw
            staticFaceExtractor = new StaticGestureDataExtract();
            staticBodyExtractor = new StaticGestureDataExtract();
            _bodyVideo = new ArrayList();
            _faceVideo = new ArrayList();
            _handVideo = new ArrayList();
            _recordedVideo = new ArrayList();
            _faceOrientationVideo = new ArrayList();
            fHandTracker = new FHandTracker();
            FaceTrackingViewer.meshDisabled = true;
        }


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //kinect discovery and initialization



        }
        private void WindowClosed(object sender, EventArgs e)
        {
            this.Kinect = null;
            Environment.Exit(0);
        }
        #endregion ctor + Window Events
        #region Kinect discovery + set up
        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                //enable skeleton stream
                sensor.SkeletonStream.Enable();
                _FrameSkeletons = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];
                sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(NuiSkeletonFrameReady);
                //sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(StartRecognizing);
                //enable color stream
                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this._ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth,
                                                colorStream.FrameHeight, 96, 96,
                                                PixelFormats.Bgr32, null);
                this._ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth,
                colorStream.FrameHeight);
                this._ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;

                videoImage.Source = this._ColorImageBitmap;
                FaceImage.Source = this._ColorImageBitmap;
                sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(NuiColorFrameReady);
                //Dtw events
                sensor.SkeletonFrameReady += SkeletonExtractSkeletonFrameReady;
                Skeleton2DDataExtract.Skeleton2DdataCoordReady += NuiSkeleton2DdataCoordReady;


                //  sensor.Start();
            }

            if (sensor != null)
            {
                try
                {
                    // sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    //sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        // sensor.DepthStream.Range = DepthRange.Near;
                        // sensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        //      sensor.DepthStream.Range = DepthRange.Default;
                        //      sensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    //sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    // sensor.SkeletonStream.Enable();

                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }
        private void UninitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.SkeletonFrameReady -= NuiSkeletonFrameReady;
                sensor.ColorFrameReady -= NuiColorFrameReady;
            }
        }
        public KinectSensor Kinect
        {
            get { return this._Kinect; }
            set
            {
                if (this._Kinect != null)
                {
                    UpdateDisplayStatus("No connected device.");
                    UninitializeKinectSensor(this._Kinect);
                    this._Kinect = null;
                }
                if (value != null && value.Status == KinectStatus.Connected)
                {
                    this._Kinect = value;
                    InitializeKinectSensor(this._Kinect);
                    kinectStatus.Text = string.Format("{0} - {1}", this._Kinect.UniqueKinectId, this._Kinect.Status);
                }
                else
                {
                    UpdateDisplayStatus("No connected device.");
                }
            }
        }
        private void DiscoverKinectSensor(KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = kinectChangedEventArgs.NewSensor;
            SliderValue.Text = "Tilt angle : " + _Kinect.ElevationAngle;
            TiltSlider.Value = _Kinect.ElevationAngle;
            // this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.Kinect == null)
                    {
                        this.Kinect = e.Sensor;
                        UpdateDisplayStatus("Sensor connected.");
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (this.Kinect == e.Sensor)
                    {
                        this.Kinect = null;
                        this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                        if (this.Kinect == null)
                        {
                            UpdateDisplayStatus("No connected device.");
                        }
                    }
                    break;
                //TODO: Handle all other statuses according to needs
            }
            if (e.Status == KinectStatus.Connected)
            {
                this.Kinect = e.Sensor;
            }
        }
        private void UpdateDisplayStatus(string message)
        {
            kinectStatus.Text = "Kinect: " + message;
        }
        #endregion Kinect discovery + set up
        #region KinectEventsMethods
        private void NuiSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            facePoints3D = FaceTrackingViewer.facePointS3D;
            if (facePoints3D != null)
            {
                if (facePoints3D[FeaturePoint.AboveChin] != null)
                {
                    //Debug.WriteLine(FaceDataExtract.ProcessData(facePointsS)[1]);
                }
            }
            /*if (!(bool)seated.IsChecked)
            {
                _Kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }*/
            /*  if (!(bool)faceTracking.IsChecked)
              {

                  _Kinect.DepthStream.Disable();
              }*/

            _Kinect.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            //this.faceOrientation.Text = "Face Orientation X = " + FaceTrackingViewer.x + " Y = " + FaceTrackingViewer.y + " Z = " + FaceTrackingViewer.z;
            //this.faceOrientationText.Text = " Face Orientation X = " + FaceTrackingViewer.x;
            var orientationString = FaceDataExtract.getFaceLookingPosition(FaceTrackingViewer.x, FaceTrackingViewer.y, FaceTrackingViewer.z);



            //this.faceOrientationText.Text = "Face Orientation: "+orientationString;
            //mouse.moveMouse(-FaceTrackingViewer.x, -FaceTrackingViewer.y);



            //    if ((bool)captureBody.IsChecked)
            //    {
            // Debug.WriteLine(captureBody);

            //   }
            /*  if (!(bool)faceMesh.IsChecked)
              {
                  FaceTrackingViewer.meshDisabled = false;
              }
              else
              {
                  FaceTrackingViewer.meshDisabled = true;
              }*/
            /* if (!(bool)nearTracking.IsChecked)
             {
                 _Kinect.DepthStream.Range = DepthRange.Default;
                 _Kinect.SkeletonStream.EnableTrackingInNearRange = false;
             }*/
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    frame.CopySkeletonDataTo(_FrameSkeletons);
                    Skeleton data = (from s in _FrameSkeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();
                    if (data != null)
                    {
                        Brush brush = new SolidColorBrush(Colors.Blue);
                        Brush brushRed = new SolidColorBrush(Colors.Red);
                        Brush brushGreen = new SolidColorBrush(Colors.Green);
                        Brush brushDarkRed = new SolidColorBrush(Colors.DarkRed);
                        Brush brushGold = new SolidColorBrush(Colors.Gold);
                        skeletonCanvas.Children.Clear();
                        canvas1.Children.Clear();
                        //Draw bones
                        /*if (!(bool)seated.IsChecked)
                        {
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.HipCenter, JointType.Spine, JointType.ShoulderCenter, JointType.Head));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight));
                        }*/



                        skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.Head));
                        skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft));
                        skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight));

                        // Draw joints
                        foreach (Joint joint in data.Joints)
                        {
                            if (joint.TrackingState == JointTrackingState.NotTracked)
                            {
                                continue;
                            }
                            System.Windows.Point jointPos = GetDisplayPosition(joint);

                            // Debug.WriteLine(jointPos.X + "  " + jointPos.Y);
                            Ellipse ellipse = new Ellipse
                            {
                                Fill = brushRed,
                                Width = 10,
                                Height = 10,
                                Margin = new Thickness(jointPos.X, jointPos.Y, 0, 0)

                            };
                            skeletonCanvas.Children.Add(ellipse);
                        }
                        //   if ((bool)enableHandTracking.IsChecked)
                        //   {
                        fHandTracker.enable();
                        Hand hand;

                        List<Hand> hands = fHandTracker.hands;
                        /*
                                                if (hands.Count != 0)
                                                {
                                                     hand = hands.ElementAt(0);

                                                     var tmp = new double[hand.fingertips3D.Count * 3];
                                                     for (int i = 0; i < hand.fingertips3D.Count; i++)
                                                     {
                                                         tmp[3 * i] = hand.fingertips3D.ElementAt(i).X;
                                                         tmp[(3 * i) + 1] = hand.fingertips3D.ElementAt(i).Y;
                                                         tmp[(3 * i) + 2] = hand.fingertips3D.ElementAt(i).Z;
                                                     }

                                                     _handVideo.Add(tmp);
                        
                                                }
                         */

                        float inm = 1.9F;
                        handStaticGesture.Text = " Hands: " + hands.Count();
                        handGestureText.Text = "";




                        for (int i = 0; i < hands.Count; i++)
                        {
                            handGestureText.Text = handGestureText.Text + "hand " + i + " : " + hands[i].fingertips.Count + " f, ";
                            for (int j = 0; j < hands[i].fingertips.Count; j++)
                            {
                                //    System.IO.File.AppendAllText("@fingertips2.txt", "X: " + hands[i].fingertips3D[j].X.ToString() + Environment.NewLine);
                                //    System.IO.File.AppendAllText("@fingertips2.txt", "Y: " + hands[i].fingertips3D[j].Y.ToString() + Environment.NewLine);
                                //    System.IO.File.AppendAllText("@fingertips2.txt", "Z: " + hands[i].fingertips3D[j].Z.ToString() + Environment.NewLine);



                                Ellipse ellipse = new Ellipse
                                {
                                    Fill = brushGreen,
                                    Width = 15,
                                    Height = 15,
                                    Margin = new Thickness((float)(hands[i].fingertips[j].Y) * inm, (float)(hands[i].fingertips[j].X) * inm, 0, 0)

                                };
                                canvas1.Children.Add(ellipse);
                            }
                            Ellipse ellipseCenter = new Ellipse
                            {
                                Fill = brushGold,
                                Width = 15,
                                Height = 15,
                                Margin = new Thickness((float)(hands[i].palm.Y) * inm, (float)(hands[i].palm.X) * inm, 0, 0)

                            };
                            canvas1.Children.Add(ellipseCenter);
                            Int32Rect rect = new Int32Rect(0, 0, 2, 2);
                            int size = rect.Width * rect.Height * 4;
                            // if ((bool)handContour.IsChecked)
                            // {
                            for (int j = 0; j < hands[i].contour.Count - 6; j = j + 6)
                            {

                                PointFT p = hands[i].contour[j];
                                Ellipse ellipse = new Ellipse
                                {
                                    Fill = brushDarkRed,
                                    Width = 3,
                                    Height = 3,
                                    Margin = new Thickness((float)(p.Y) * inm, (float)(p.X) * inm, 0, 0)

                                };
                                canvas1.Children.Add(ellipse);
                            }
                            //}

                        }
                        //    }
                        /*  else
                          {
                              fHandTracker.disable();
                          }*/
                    }
                }
            }

            //   using()

        }
        private void NuiColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, pixelData,
                    this._ColorImageStride, 0);
                }
            }

        }
        private Polyline GetBodySegment(JointCollection joints, Brush brush, params JointType[] ids)
        {
            PointCollection points = new PointCollection(ids.Length);
            for (int i = 0; i < ids.Length; ++i)
            {
                points.Add(GetDisplayPosition(joints[ids[i]]));
            }
            Polyline polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 2;
            return polyline;
        }
        private System.Windows.Point GetDisplayPosition(Joint joint)
        {
            ColorImagePoint colorImgpoint = Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, ColorImageFormat.RgbResolution640x480Fps30);
            return new System.Windows.Point(colorImgpoint.X, colorImgpoint.Y);
        }

        #endregion KinectEventsMethods
        #region DTWGestureRecognition
        private bool LoadGesturesFromFile(string fileLocation, DtwGestureRecognizer dtw, int dim)
        {
            try
            {
                int itemCount = 0;
                string line;
                string gestureName = String.Empty;
                // TODO I'm defaulting this to 12 here for now as it meets my current need but I need to cater for variable lengths in the future
                ArrayList frames = new ArrayList();
                double[] items = new double[dim];
                // Read the file and display it line by line.
                System.IO.StreamReader file = new System.IO.StreamReader(fileLocation);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("@"))
                    {
                        gestureName = line;
                        continue;
                    }
                    if (line.StartsWith("~"))
                    {
                        frames.Add(items);
                        itemCount = 0;
                        items = new double[dim];
                        continue;
                    }
                    if (!line.StartsWith("----"))
                    {
                        items[itemCount] = Double.Parse(line);
                    }
                    itemCount++;
                    if (line.StartsWith("----"))
                    {
                        dtw.AddOrUpdate(frames, gestureName, eachLabelRecordingNb);
                        frames = new ArrayList();
                        gestureName = String.Empty;
                        itemCount = 0;
                    }
                }
                file.Close();
            }
            catch (Exception exp)
            {
                return false;
            }
            return true;
        }
        private static void SkeletonExtractSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonFrame.CopySkeletonDataTo(_FrameSkeletons);
                    Skeleton data = (from s in _FrameSkeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();
                    Skeleton2DDataExtract.ProcessData(data);
                }
            }
        }

        private void NuiSkeleton2DdataCoordReady(object sender, Skeleton2DdataCoordEventArgs a)
        {
            currentBufferFrame.Text = "Current Buffer Frame: " + _bodyVideo.Count.ToString();

            // We need a sensible number of frames before we start attempting to match gestures against remembered sequences
            //_bodyVideo.Add(a.GetCoords());
            if (_bodyVideo.Count > MinimumFrames && _capturing == false)
            {
                ////Debug.WriteLine("Reading and video.Count=" + video.Count);
                //  string s = _dtw1.Recognize(_bodyVideo);

                // results.Text = "Body - Gesture: " + s;
                //reader.Speak(s);
                //      if (test.IsPressed) { }

                /*          if (!s.Contains("UNKNOWN"))
                          {
                              // There was no match so reset the buffer
                              _bodyVideo = new ArrayList();
                          }
                  */
            }
            //         if (_faceVideo.Count > MinimumFrames && _capturing == false)
            //        {
            ////Debug.WriteLine("Reading and video.Count=" + video.Count);
            //  string s = _dtw2.Recognize(_faceVideo);
            //faceResults.Text = "Face - Gesture: " + s;
            /* if ((bool)enableNui.IsChecked)
             {
                 GestureInterfaceControl.executeGesture(s);
             }*/
            /*if (!s.Contains("UNKNOWN"))
            {
                // There was no match so reset the buffer
                _faceVideo = new ArrayList();
            }*/
            //      }
            //     if (_faceOrientationVideo.Count > MinimumFrames && _capturing == false)
            //   {
            ////Debug.WriteLine("Reading and video.Count=" + video.Count);
            //     string s;
            //_dtw3.Recognize(_faceOrientationVideo);
            //faceDynamicOrientation.Text = "Face - Dynamic Orientation: " + s;
            /* if ((bool)enableNui.IsChecked)
             {
                 GestureInterfaceControl.executeGesture(s);
             }*/
            /*    if (!s.Contains("UNKNOWN"))
                {
                    // There was no match so reset the buffer
                    _faceOrientationVideo = new ArrayList();
                }*/
            //   }

            if (_handVideo.Count > MinimumFrames && _capturing == false)
            {


                //Debug.WriteLine("Reading and video.Count=" + _handVideo.Count);
                //  string s = _dtw4.Recognize(_handVideo);
                //  results.Text = "Body - Gesture: " + s;
                //faceDynamicOrientation.Text = "Face - Dynamic Orientation: " + s;
                /* if ((bool)enableNui.IsChecked)
                 {
                     GestureInterfaceControl.executeGesture(s);
                 }*/
                /*   if (!s.Contains("UNKNOWN"))
                   {
                       // There was no match so reset the buffer
                       _handVideo = new ArrayList();
                   } */
            }

            // Ensures that we remember only the last x frames
            if (_bodyVideo.Count > BufferSize)
            {
                // If we are currently capturing and we reach the maximum buffer size then automatically store
                if (_capturing)
                {
                    if ((bool)captureBody.IsChecked)
                    {
                        DtwStoreClick(null, null);
                    }
                    /*  else if ((bool)captureBodyS.IsChecked)
                      {
                          DtwStoreClick(null, null);
                      } */
                }
                else
                {
                    // Remove the first frame in the buffer
                    _bodyVideo.RemoveAt(0);
                }
            }
            /*     if (_faceVideo.Count > BufferSize)
                 {
                     // If we are currently capturing and we reach the maximum buffer size then automatically store
                     if (_capturing)
                     {
                         if ((bool)captureFace.IsChecked)
                         {
                             DtwStoreClick(null, null);
                         }
                       /*  else if ((bool)captureFaceS.IsChecked)
                         {
                             DtwStoreClick(null, null);
                         } */

            //       }
            /*      else
                  {
                      // Remove the first frame in the buffer
                      _faceVideo.RemoveAt(0);
                  }
              }
              */
            if (_handVideo.Count > BufferSize)
            {
                // If we are currently capturing and we reach the maximum buffer size then automatically store
                if (_capturing)
                {
                    if ((bool)captureHand.IsChecked)
                    {
                        DtwStoreClick(null, null);
                    }
                    /*  else if ((bool)captureFaceS.IsChecked)
                      {
                          DtwStoreClick(null, null);
                      } */

                }
                else
                {
                    // Remove the first frame in the buffer
                    _handVideo.RemoveAt(0);
                }
            }

            /*       if (_faceOrientationVideo.Count > BufferSize)
                   {
                       // If we are currently capturing and we reach the maximum buffer size then automatically store
                       if (_capturing)
                       {
                          /* if ((bool)captureFaceO.IsChecked)
                           {
                               DtwStoreClick(null, null);
                           } */
            /*         }
                     else
                     {
                         // Remove the first frame in the buffer
                         _faceOrientationVideo.RemoveAt(0);
                     }
                 }
              */
            // Decide which skeleton frames to capture. Only do so if the frames actually returned a number. 
            // For some reason my Kinect/PC setup didn't always return a double in range (i.e. infinity) even when standing completely within the frame.
            // TODO Weird. Need to investigate this

            //   bool[][] near = FHandTracker.generateValidMatrix(frame, distances);

            Hand hand;

            List<Hand> hands = fHandTracker.hands;
            if (hands.Count != 0)
            {
                hand = hands.ElementAt(0);

                var tmp = new double[hand.fingertips3D.Count * 3];
                for (int i = 0; i < hand.fingertips3D.Count; i++)
                {
                    tmp[3 * i] = hand.fingertips3D.ElementAt(i).X;
                    tmp[(3 * i) + 1] = hand.fingertips3D.ElementAt(i).Y;
                    tmp[(3 * i) + 2] = hand.fingertips3D.ElementAt(i).Z;
                }

                storeCoords2 = tmp;
                _handVideo.Add(storeCoords2);
                // if (test2.IsPressed) { }
            }

            
           

            /*          double aux = a.GetPoint(0).X;
                      if (!double.IsNaN(aux))
                      {
                          // Optionally register only 1 frame out of every n
                          _flipFlop = (_flipFlop + 1) % Ignore;
                          if (_flipFlop == 0)
                          {
                              //   if ((bool)faceTracking.IsChecked)
                              //   {


                              /*      if (FaceTrackingViewer.animationUnits != null)
                                    {
                                        auxFaceAnimationUnit = FaceDataExtract.ProcessData(FaceTrackingViewer.animationUnits);
                                        _faceVideo.Add(FaceDataExtract.ProcessData(FaceTrackingViewer.animationUnits));
                                        _faceOrientationVideo.Add(FaceDataExtract.ProcessOrientationData()); //because when we have aninationUnits available we also have the orientation
                                        for (int i = 0; i < auxFaceAnimationUnit.Length; i++)
                                        {
                                            auxFaceAnimationUnit[i] = (int)(auxFaceAnimationUnit[i] * 10000);
                                        }
                                        if (staticFaceInit)
                                        {
                                            double[] percentages = new double[staticFaceGestures.Length];
                                            acFace.ClassifyInstance(auxFaceAnimationUnit, out percentages);
                                            double minF = 0;
                                            int indexF = -1;
                                            for (int i = 0; i < percentages.Length; i++)
                                            {

                                                if (percentages[i] > minF)
                                                {
                                                    minF = percentages[i];
                                                    indexF = i;
                                                }
                                            }

                                            if (indexF != -1 &&(minF>0.9))
                                            {
                                                //faceResultsS.Text = "Face - Static Gesture: " + staticFaceGestures[indexF];
                                            }
                                            else if ((minF < 0.9))
                                            {
                                                //faceResultsS.Text = "Face - Static Gesture: UNKNOWN";
                                            }
                                        }
                                   }
                       //         }*/
            //   {


            storeCoords = a.GetCoords();
            _bodyVideo.Add(storeCoords);



            /*    auxBodyVideo = a.GetCoords();
                for (int i = 0; i < auxBodyVideo.Length; i++)
                {
                    auxBodyVideo[i] = (int)(auxBodyVideo[i] * 10000);
                    //    Debug.WriteLine(auxBodyVideo[i]);
                }
                if (staticBodyInit)
                {
                    double[] percentages = new double[staticFaceGestures.Length];
                    acBody.ClassifyInstance(auxBodyVideo, out percentages);
                    double min = 0;
                    int index = -1;
                    for (int i = 0; i < percentages.Length; i++)
                    {
                        //  Debug.WriteLine(percentages[i]);
                        if (percentages[i] > min)
                        {
                            min = percentages[i];
                            index = i;
                        }
                    }
                    if (index != -1 && (min > 0.9))
                    {
                       // resultsS.Text = "Body - Static Gesture: " + staticBodyGestures[index];
                    }
                    else if ((min < 0.9))
                    {
                       // resultsS.Text = "Body - Static Gesture: UNKNOWN";
                    }
                }
         //   }*/

            //            }
            //         }
            // Update the debug window with Sequences information
            //dtwTextOutput.Text = _dtw1.RetrieveText();
        }






        double difference = 0;
        public double NormalizeData()
        {
            ArrayList _seq = _dtw1.getSequence();
            List<double> xList = new List<double>();

            for (int i = 0; i < _seq.Count; i++)
            {
                //    difference = 0;
                foreach (double[] storedframe in (ArrayList)_seq[0])
                {
                    foreach (double dub in (double[])storedframe)
                    {
                        xList.Add(dub); // first add all the 18 coordinates present in each of 34 frames.
                    }

                }
            }

            List<double> storeFirstX = new List<double>();

            for (int j = 0; j <= xList.Count; j++)
            {
                if (j <= 31)
                    storeFirstX.Add(xList[18 * j]); // out of all coordinates, just store the x coordinates inside of storeX, used 3 as every x coordinate is stored at 0,3,6,9.. index
            }

            // Going to do the same for _bodyVideo now

            List<double> xSecondList = new List<double>();
            List<double> storeSecondX = new List<double>();

            for (int k = 0; k < _bodyVideo.Count; k++)
            {
                foreach (double realframe in (double[])_bodyVideo[k])
                {
                    xSecondList.Add(realframe);
                }
            }

            for (int m = 0; m < xSecondList.Count; m++)
            {
                if (m <= 31)
                    storeSecondX.Add(xSecondList[18 * m]);
            }

            // can use storeFirstX and storeSecondX to calculate the difference...                
            for (int k = 0; k < storeSecondX.Count; k++)
            {
                difference = difference + (storeFirstX[k] - storeSecondX[k]);
            }

            xList.Clear();
            xSecondList.Clear();
            storeFirstX.Clear();
            storeSecondX.Clear();

            return difference;

        }

        private void DtwReadClick(object sender, RoutedEventArgs e)
        {
            // Set the buttons enabled state
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = true;
            dtwStore.IsEnabled = false;
            // Set the capturing? flag
            _capturing = false;
            // Update the status display
            status.Text = "Status: Reading";
        }
        private void DtwCaptureClick(object sender, RoutedEventArgs e)
        {
            _captureCountdown = DateTime.Now.AddSeconds(CaptureCountdownSeconds);
            _captureCountdownTimer = new System.Windows.Forms.Timer();
            _captureCountdownTimer.Interval = 50;
            _captureCountdownTimer.Start();
            _captureCountdownTimer.Tick += CaptureCountdown;
        }
        private void CaptureCountdown(object sender, EventArgs e)
        {
            if (sender == _captureCountdownTimer)
            {
                if (DateTime.Now < _captureCountdown)
                {
                    status.Text = "Status: Wait " + ((_captureCountdown - DateTime.Now).Seconds + 1) + " seconds";
                }
                else
                {
                    _captureCountdownTimer.Stop();
                    /*    if ((bool)captureFace.IsChecked)
                        {
                            status.Text = "Status: Recording face movement";
                        }
                      /*  else if ((bool)captureFaceO.IsChecked)
                        {
                            status.Text = "Status: Recording face orientation gesture";
                        } */
                    if ((bool)captureBody.IsChecked)
                    {
                        status.Text = "Status: Recording body gesture";
                    }
                    else if ((bool)captureHand.IsChecked)
                    {
                        status.Text = "Status: Recording hand gesture";
                    }
                    /*  else if ((bool)captureHandS.IsChecked)
                      {
                          status.Text = "Status: Recording hand static gesture";
                      } */
                    /* else if ((bool)captureBodyS.IsChecked)
                     {
                         status.Text = "Status: Recording body static gesture";
                     } */
                    /* else if ((bool)captureFaceS.IsChecked)
                     {
                         status.Text = "Status: Recording face static gesture";
                     } */
                    // else
                    // {
                    status.Text = "Status: Recording gesture";
                    // }
                    StartCapture();
                }
            }
        }

        private void CaptureCountdown2(object sender, EventArgs e)
        {
            if (sender == _captureCountdownTimer)
            {
                if (DateTime.Now < _captureCountdown)
                {
                    status.Text = "Status: Wait " + ((_captureCountdown - DateTime.Now).Seconds + 1) + " seconds";
                }
                else
                {
                    _captureCountdownTimer.Stop();
                    /*    if ((bool)captureFace.IsChecked)
                        {
                            status.Text = "Status: Recording face movement";
                        }
                      /*  else if ((bool)captureFaceO.IsChecked)
                        {
                            status.Text = "Status: Recording face orientation gesture";
                        } */
                    if ((bool)captureBody.IsChecked)
                    {
                        status.Text = "Status: Recording body gesture";
                    }
                    else if ((bool)captureHand.IsChecked)
                    {
                        status.Text = "Status: Recording hand gesture";
                    }
                    /*  else if ((bool)captureHandS.IsChecked)
                      {
                          status.Text = "Status: Recording hand static gesture";
                      } */
                    /* else if ((bool)captureBodyS.IsChecked)
                     {
                         status.Text = "Status: Recording body static gesture";
                     } */
                    /* else if ((bool)captureFaceS.IsChecked)
                     {
                         status.Text = "Status: Recording face static gesture";
                     } */
                    // else
                    // {
                    status.Text = "Status: Recording gesture";
                    // }


                    /* Skeleton2DDataExtract.Skeleton2DdataCoordReady += */
                    StartRecognizing();
                }
            }
        }

        private void CaptureCountdown3(object sender, EventArgs e)
        {
            if (sender == _captureCountdownTimer)
            {
                if (DateTime.Now < _captureCountdown)
                {
                    status.Text = "Status: Wait " + ((_captureCountdown - DateTime.Now).Seconds + 1) + " seconds";
                }
                else
                {
                    _captureCountdownTimer.Stop();
                    /*    if ((bool)captureFace.IsChecked)
                        {
                            status.Text = "Status: Recording face movement";
                        }
                      /*  else if ((bool)captureFaceO.IsChecked)
                        {
                            status.Text = "Status: Recording face orientation gesture";
                        } */
                    if ((bool)captureBody.IsChecked)
                    {
                        status.Text = "Status: Recording body gesture";
                    }
                    else if ((bool)captureHand.IsChecked)
                    {
                        status.Text = "Status: Recording hand gesture";
                    }
                    /*  else if ((bool)captureHandS.IsChecked)
                      {
                          status.Text = "Status: Recording hand static gesture";
                      } */
                    /* else if ((bool)captureBodyS.IsChecked)
                     {
                         status.Text = "Status: Recording body static gesture";
                     } */
                    /* else if ((bool)captureFaceS.IsChecked)
                     {
                         status.Text = "Status: Recording face static gesture";
                     } */
                    // else
                    // {
                    status.Text = "Status: Recording gesture";
                    // }


                    /* Skeleton2DDataExtract.Skeleton2DdataCoordReady += */
                    StartRecognizingForHands();
                }
            }
        }



        private void StartRecognizing(/*object sender, Skeleton2DdataCoordEventArgs a*/)
        {

            string s;
            int i = 0;
            _bodyVideo = new ArrayList();


            currentBufferFrame.Text = "Current Buffer Frame: " + _bodyVideo.Count.ToString();
            while (i < 34)
            {
                //Skeleton2DDataExtract.Skeleton2DdataCoordReady += NuiSkeleton2DdataCoordReady;
                _bodyVideo.Add(storeCoords);
                i++;
            }

            s = _dtw1.Recognize(_bodyVideo);
            results.Text = "Body - Gesture: " + s;
            reader.Speak(s.Replace("@", ""));
            _bodyVideo.Clear();

        }

        private void StartRecognizingForHands(/*object sender, Skeleton2DdataCoordEventArgs a*/)
        {

            string s;
            int i = 0;
            _handVideo = new ArrayList();


            currentBufferFrame.Text = "Current Buffer Frame: " + _handVideo.Count.ToString();
            while (i < 34)
            {
               // Skeleton2DDataExtract.Skeleton2DdataCoordReady += NuiSkeleton2DdataCoordReady;
                _handVideo.Add(storeCoords2);
                i++;
            }

            s = _dtw4.Recognize(_handVideo);
            results.Text = "Body - Gesture: " + s;
            reader.Speak(s.Replace("@", ""));
            _handVideo.Clear();

        }


        private void StartCapture()
        {
            // Set the buttons enabled state
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = false;
            dtwStore.IsEnabled = true;
            // Set the capturing? flag
            _capturing = true;

            ////_captureCountdownTimer.Dispose();

            // Clear the _video buffer and start from the beginning
            _bodyVideo = new ArrayList();
            _faceVideo = new ArrayList();
            _faceOrientationVideo = new ArrayList();
            _handVideo = new ArrayList();

        }
        private void DtwStoreClick(object sender, RoutedEventArgs e)
        {
            // Set the buttons enabled state.
            DtwGestureRecognizer dtw;
            ArrayList _video;
            /*    if ((bool)captureFace.IsChecked)
                {
                    dtw = _dtw2;
                    _video = _faceVideo;
                    // Add the current video buffer to the dtw sequences list
                    dtw.AddOrUpdate(_video, gestureList.Text, eachLabelRecordingNb);
                   // faceResults.Text = "Gesture " + gestureList.Text + " added";
               */
            if ((bool)captureBody.IsChecked)
            {
                dtw = _dtw1;
                _video = _bodyVideo;
                // Add the current video buffer to the dtw sequences list
                dtw.AddOrUpdate(_video, gestureList.Text, eachLabelRecordingNb);
                results.Text = "Gesture " + gestureList.Text + " added";
            }
            else if ((bool)captureHand.IsChecked)
            {
                dtw = _dtw4;
                _video = _handVideo;

                dtw.AddOrUpdate(_video, gestureList.Text, eachLabelRecordingNb);
                results.Text = "Gesture " + gestureList.Text + "added";

            }
            /* else if ((bool)captureFaceO.IsChecked)
             {
                 dtw = _dtw3;
                 _video = _faceOrientationVideo;
                 // Add the current video buffer to the dtw sequences list
                 dtw.AddOrUpdate(_video, gestureList.Text, eachLabelRecordingNb);
                 //faceDynamicOrientation.Text = "Gesture " + gestureList.Text + " added";
             } */
            /* else if ((bool)captureFaceS.IsChecked)
             {
                 staticFaceExtractor.AddOrUpdate(_faceVideo, gestureList.Text, eachLabelRecordingNb);
                 _faceVideo = new ArrayList();
                 _video = _faceVideo;
             } */
            /*  else if ((bool)captureBodyS.IsChecked)
              {
                  staticBodyExtractor.AddOrUpdate(_bodyVideo, gestureList.Text, eachLabelRecordingNb);
                  _bodyVideo = new ArrayList();
                  _video = _bodyVideo;
              } */

            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = true;
            dtwStore.IsEnabled = false;
            // Set the capturing? flag
            _capturing = false;
            status.Text = "Status: Remembering " + gestureList.Text;
            // Scratch the _video buffer
            _video = new ArrayList();
            // Switch back to Read mode
            DtwReadClick(null, null);
        }
        private void DtwSaveToFile(object sender, RoutedEventArgs e)
        {
            /* if ((bool)captureFace.IsChecked)
             {
                 string fileName = GestureFaceSaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
                 status.Text = "Status: Saved to " + fileName;
                 System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, _dtw2.RetrieveText());
             } */
            /*  else if ((bool)captureFaceO.IsChecked)
              {
                  string fileName = GestureFaceOrientationSaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
                  status.Text = "Status: Saved to " + fileName;
                  System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, _dtw3.RetrieveText());
              } */
            if ((bool)captureBody.IsChecked)
            {
                string fileName = GestureBodySaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
                status.Text = "Status: Saved to " + fileName;
                System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, _dtw1.RetrieveText());
            }
            /*  else if ((bool)captureFaceS.IsChecked)
              {
                  string fileName = GestureStaticFaceSaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".arff";
                  status.Text = "Status: Saved to " + fileName;
                  System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, staticFaceExtractor.RetrieveText(staticFaceAttributes, staticFaceGestures, "staticFaceGestures"));
              } */
            /*else if ((bool)captureBodyS.IsChecked)
            {
                string fileName = GestureStaticBodySaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".arff";
                status.Text = "Status: Saved to " + fileName;
                System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, staticBodyExtractor.RetrieveText(staticBodyAttributes,staticBodyGestures, "staticBodyGestures"));  
            } */
            else if ((bool)captureHand.IsChecked)
            {
                string fileName2 = GestureHandSaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
                status.Text = "Status: Saved to " + fileName2;
                System.IO.File.WriteAllText(GestureSaveFileLocation + fileName2, _dtw4.RetrieveText());
            }

            /* else if ((bool)captureHandS.IsChecked)
             {
                 string fileName = GestureStaticHandSaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".arff";
                 status.Text = "Status: Saved to " + fileName;
                 System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, staticBodyExtractor.RetrieveText(staticBodyAttributes, staticBodyGestures, "staticHandGestures"));
             } */

            /*  else
              {
                  status.Text = "Status: Please select what type of dynamic gestures to save";
              } */
        }

        private void DtwLoadFile(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension
            //_Kinect.ElevationAngle = 10;
            /* if ((bool)captureFaceS.IsChecked || (bool)captureBodyS.IsChecked || (bool)captureHandS.IsChecked)
             {
                 dlg.DefaultExt = ".model";
                 dlg.Filter = "Text documents (.model)|*.model";
             } */
            //  else
            //  {
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";
            //  }
            dlg.InitialDirectory = GestureSaveFileLocation;
            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                if ((bool)captureBody.IsChecked)
                {
                    bool loaderResult = LoadGesturesFromFile(dlg.FileName, _dtw1, 18);
                    if (loaderResult)
                    {
                        status.Text = "Status: Body - Gesture loaded";
                        // String s = _dtw1.Recognize(_bodyVideo);
                        // results.Text = "Body - Gesture: " + s;
                    }
                    else
                    {
                        status.Text = "Status: Invalid Body - Gesture File";
                    }
                }
                /* else if ((bool)captureFace.IsChecked)
                 {
                     bool loaderResult = LoadGesturesFromFile(dlg.FileName, _dtw2, 6);
                     if (loaderResult)
                     {
                         status.Text = "Status: Face - Gesture loaded";
                     }
                     else
                     {
                         status.Text = "Status: Invalid Face - Gesture File";
                     }
                 }
                 */

                else if ((bool)captureHand.IsChecked)
                {
                    bool load = LoadGesturesFromFile(dlg.FileName, _dtw4, 15);

                    if (load)
                    {

                        status.Text = "Status : Hands - Gestureloaded";

                    }
                    else
                    {
                        status.Text = "Status: Invalid hand - Gesture File";
                    }

                }
                /*   else if ((bool)captureFaceO.IsChecked)
                   {
                       bool loaderResult = LoadGesturesFromFile(dlg.FileName, _dtw3, 3);
                       if (loaderResult)
                       {
                           status.Text = "Status: Face - Dynamic Orientation Gestures loaded";
                       }
                       else
                       {
                           status.Text = "Status: Invalid Face - Dynamic Orientation Gestures File";
                       }
                   } */
                /* else if ((bool)captureFaceS.IsChecked)
                 {        
                     acFace = new Weka.BayesNaive();
                     string[] attributes = staticFaceAttributes;
                     string[] gestures = staticFaceGestures;
                     string classAttribute = "gesture";
                     string modelLocation = dlg.FileName;
                     acFace.InitializeClassifier(attributes,gestures,classAttribute,modelLocation);
                     staticFaceInit = true;                  
                     status.Text = "The Weka model " + dlg.FileName + " was loaded";
                 } */
                /* else if ((bool)captureBodyS.IsChecked)
                 {
                     acBody = new Weka.BayesNaive();
                     string[] atributes = staticBodyAttributes;
                     string[] gestures = staticBodyGestures;
                     string classAttribute = "gesture";
                     string modelLocation = dlg.FileName;
                     acBody.InitializeClassifier(atributes, gestures, classAttribute, modelLocation);
                     staticBodyInit = true;   
                     status.Text = "The Weka model " + dlg.FileName + " was loaded";
                 } */
                /* else
                 {
                     status.Text = "Status: Please select what type of gestures to load";
                 }*/
                //dtwTextOutput.Text = _dtw.RetrieveText();

            }
        }
        private void DtwShowGestureText(object sender, RoutedEventArgs e)
        {
            //dtwTextOutput.Text = _dtw.RetrieveText();
        }
        #endregion DTWGestureRecognition

        private void enableNui_Checked(object sender, RoutedEventArgs e)
        {

        }

        //  private void checkBox1_Checked(object sender, RoutedEventArgs e)
        // {
        // if ((bool)faceTracking.IsChecked)
        //{

        //  _Kinect.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
        //}
        /*else
        {

        }*/
        // }

        private void seated_Checked(object sender, RoutedEventArgs e)
        {
            /* if ((bool)seated.IsChecked)
             {
                 _Kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                 Debug.WriteLine("seated");
             }
             else
             {
                  Debug.WriteLine("not seated");
             }*/
        }

        private void nearTracking_Checked(object sender, RoutedEventArgs e)
        {
            /*  if ((bool)nearTracking.IsChecked)
              {
                  _Kinect.DepthStream.Range = DepthRange.Near;
                  _Kinect.SkeletonStream.EnableTrackingInNearRange = true;
              }
              else
              {
                  _Kinect.DepthStream.Range = DepthRange.Default;
                  _Kinect.SkeletonStream.EnableTrackingInNearRange = false;
              }*/
        }

        private void faceOrientation_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
        }
        private void face_movement_checked(object sender, RoutedEventArgs e)
        {
        }
        private void checkBox1_Checked_1(object sender, RoutedEventArgs e)
        {
        }
        private void faceMesh_checked(object sender, RoutedEventArgs e)
        {
        }

        private void TiltSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)TiltSlider.Value;
            SliderValue.Text = "Tilt Angle : " + value + "";
            BackgroundWorker barInvoker = new BackgroundWorker();
            barInvoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                try
                {
                    _Kinect.ElevationAngle = value;

                }
                catch (InvalidOperationException exp)
                {
                }
            };
            barInvoker.RunWorkerAsync();
        }

        private void checkBox1_Checked_2(object sender, RoutedEventArgs e)
        {

        }


        private void test_Click(object sender, RoutedEventArgs e)
        {

            _captureCountdown = DateTime.Now.AddSeconds(CaptureCountdownSeconds);
            _captureCountdownTimer = new System.Windows.Forms.Timer();
            _captureCountdownTimer.Interval = 50;
            _captureCountdownTimer.Start();
            _captureCountdownTimer.Tick += CaptureCountdown2;

        }

        private void normalize_Click(object sender, RoutedEventArgs e)
        {
            ArrayList _seq = _dtw1.getSequence();
            List<double> xList = new List<double>();

            for (int i = 0; i < _seq.Count; i++)
            {
                foreach (double[] storedframe in (ArrayList)_seq[0])
                {
                    foreach (double dub in (double[])storedframe)
                    {
                        xList.Add(dub); // first add all the 18 coordinates present in each of 34 frames.
                    }
                }
            }

            List<double> storeFirstX = new List<double>();

            for (int i = 0; i <= xList.Count; i++)
            {
                if (i <= 32)
                    storeFirstX.Add(xList[18 * i]); // out of all coordinates, just store the x coordinates inside of storeX, used 3 as every x coordinate is stored at 0,3,6,9.. index
            }

            // Going to do the same for _bodyVideo now

            List<double> xSecondList = new List<double>();
            List<double> storeSecondX = new List<double>();

            for (int i = 0; i < 32; i++)
            {
                foreach (double realframe in (double[])_bodyVideo[i])
                {
                    xSecondList.Add(realframe);
                }
            }

            for (int i = 0; i < xSecondList.Count; i++)
            {
                if (i <= 31)
                    storeSecondX.Add(xSecondList[18 * i]);
            }

            // can use storeFirstX and storeSecondX to calculate the difference...                
            for (int i = 0; i < storeSecondX.Count; i++)
            {
                difference = difference + (storeFirstX[i] - storeSecondX[i]);
            }
        }

        private void test_Click2(object sender, RoutedEventArgs e)
        {

            _captureCountdown = DateTime.Now.AddSeconds(CaptureCountdownSeconds);
            _captureCountdownTimer = new System.Windows.Forms.Timer();
            _captureCountdownTimer.Interval = 50;
            _captureCountdownTimer.Start();
            _captureCountdownTimer.Tick += CaptureCountdown3;


            /*  string s = _dtw4.Recognize(_handVideo);
              results.Text = "Body - Gesture: " + s;
             reader.SpeakAsync(s);
              //reader.Pause();

              candidate1.Text = _dtw4.getCandidate1();
              candidate2.Text = _dtw4.getCandidate2();
             // candidate3.Text = _dtw4.getCandidate3();

              if (!s.Contains("UNKNOWN") || s.Contains("UNKNOWN"))
              {
                  // There was no match so reset the buffer
                  _handVideo = new ArrayList();
              }
               
          }
       */


        }

    }
}

