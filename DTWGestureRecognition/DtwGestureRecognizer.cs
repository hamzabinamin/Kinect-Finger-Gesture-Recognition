using System.Diagnostics;
using System.Windows.Controls;
using System;
using System.Collections;

namespace DTWGestureRecognition
{
    using System;
    using System.Collections;    

    
    /// <summary>
    /// Dynamic Time Warping nearest neighbour sequence comparison class.
    /// Called 'Gesture Recognizer' but really it can work with any vectors
    /// </summary>
    internal class DtwGestureRecognizer
    {
        public String getCandidate1()
        {
            return Candidate1.Text;
        }

        public String getCandidate2()
        {
            return Candidate2.Text;
        }

        public String getCandidate3()
        {
            return Candidate3.Text;
        }
     

        /// <summary>
        /// Size of obeservations vectors.
        /// </summary>
        private readonly int _dimension;

        

     // System.ComponentModel.Design.

        /// <summary>
        /// Maximum distance between the last observations of each sequence.
        /// </summary>
        private readonly double _firstThreshold;

        /// <summary>
        /// Minimum length of a gesture before it can be recognised
        /// </summary>
        private readonly double _minimumLength;

        /// <summary>
        /// Maximum DTW distance between an example and a sequence being classified.
        /// </summary>
        private readonly double _globalThreshold;

        /// <summary>
        /// The gesture names. Index matches that of the sequences array in _sequences
        /// </summary>
        private readonly ArrayList _labels;

        /// <summary>
        /// Maximum vertical or horizontal steps in a row.
        /// </summary>
        private readonly int _maxSlope;

        /// <summary>
        /// The recorded gesture sequences
        /// </summary>
        private readonly ArrayList _sequences;

        /// <summary>
        /// Initializes a new instance of the DtwGestureRecognizer class
        /// First DTW constructor
        /// </summary>
        /// <param name="dim">Vector size</param>
        /// <param name="threshold">Maximum distance between the last observations of each sequence</param>
        /// <param name="firstThreshold">Minimum threshold</param>
        public DtwGestureRecognizer(int dim, double threshold, double firstThreshold, double minLen)
        {
            _dimension = dim;
            _sequences = new ArrayList();
            _labels = new ArrayList();
            _globalThreshold = threshold;
            _firstThreshold = firstThreshold;
            _maxSlope = int.MaxValue;
            _minimumLength = minLen;
        }

        /// <summary>
        /// Initializes a new instance of the DtwGestureRecognizer class
        /// Second DTW constructor
        /// </summary>
        /// <param name="dim">Vector size</param>
        /// <param name="threshold">Maximum distance between the last observations of each sequence</param>
        /// <param name="firstThreshold">Minimum threshold</param>
        /// <param name="ms">Maximum vertical or horizontal steps in a row</param>
        public DtwGestureRecognizer(int dim, double threshold, double firstThreshold, int ms, double minLen)
        {
            _dimension = dim;
            _sequences = new ArrayList();
            _labels = new ArrayList();
            _globalThreshold = threshold;
            _firstThreshold = firstThreshold;
            _maxSlope = ms;
            _minimumLength = minLen;
        }

        /// <summary>
        /// Add a seqence with a label to the known sequences library.
        /// The gesture MUST start on the first observation of the sequence and end on the last one.
        /// Sequences may have different lengths.
        /// </summary>
        /// <param name="seq">The sequence</param>
        /// <param name="lab">Sequence name</param>
        /// /// <param name="lab">maxPos maximum number of entries for a label</param>
        public void AddOrUpdate(ArrayList seq, string lab,int maxPos)
        {
            // First we check whether there is already a recording for this label. If so overwrite it, otherwise add a new entry
            int firstIndex = -1;
            int number = 0;
            

            for (int i = 0; i < _labels.Count; i++)
            {
                if ((string)_labels[i] == lab)
                {
                    if (firstIndex == -1) {
                        firstIndex = i;
                    };
                    number = number+1;
                }
            }

            // If we have a match then remove the entries at the existing index to avoid duplicates. We will add the new entries later anyway
            if (number >= maxPos)
            {
                _sequences.RemoveAt(firstIndex);
                _labels.RemoveAt(firstIndex);
            }

            // Add the new entries
            _sequences.Add(seq);
            _labels.Add(lab);
        }

       

        /// <summary>
        /// Recognize gesture in the given sequence.
        /// It will always assume that the gesture ends on the last observation of that sequence.
        /// If the distance between the last observations of each sequence is too great, or if the overall DTW distance between the two sequence is too great, no gesture will be recognized.
        /// </summary>
        /// <param name="seq">The sequence to recognise</param>
        /// <returns>The recognised gesture name</returns>

        TextBox Candidate1 = new TextBox();
        TextBox Candidate2 = new TextBox();
        TextBox Candidate3 = new TextBox();

        public string Recognize(ArrayList seq)
        {
            double minDist = double.PositiveInfinity;
            string classification = "UNKNOWN";
            double result = 0;
         //   double diff = difference;

            

          //  System.IO.File.AppendAllText("@Difference.txt", "Next Gesture" + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            for (int i = 0; i < _sequences.Count; i++)
            {

             //   System.IO.File.AppendAllText("@Difference.txt", "Next Gesture" + Environment.NewLine + Environment.NewLine + Environment.NewLine);
                var example = (ArrayList) _sequences[i];
                ////Debug.WriteLine(Dist2((double[]) seq[seq.Count - 1], (double[]) example[example.Count - 1]));
                if (Dist2((double[]) seq[seq.Count - 1], (double[]) example[example.Count - 1]) < _firstThreshold)
                {
                    double d1 = Dtw(seq, example) / example.Count;
                   // double d2 = Dtw(seq, example) / example.Count;


           /*         if (diff >= -4.2459300991 && diff <= 0.5989694392)
                    {
                        classification = "RockOnOwn";
                    }

                    else if (diff >= -22.943731317 && diff <= -7.068545632)
                    {
                        classification = "AloneOwn";
                    }
            */
                    if (d1 < minDist)
                    {
                        //Microsoft.Kinect.JointType 

                       // System.IO.File.AppendAllText("@Error3.txt", minDist - d1 + Environment.NewLine);
                        
                        minDist = d1;
                        classification = (string)_labels[i];


                        

                        //Candidate3.Text = Candidate2.Text;
                        //Candidate2.Text = Candidate1.Text;
                        //Candidate1.Text = classification;

                    }

                   
                   /* else
                    {
                       // classification = (string)_labels[i];
                        
                    } */   
                }

             //  System.IO.File.AppendAllText("@Error3.txt", "---" +Environment.NewLine);
            }
            Candidate2.Text = Candidate1.Text;
            Candidate1.Text = classification;

            return (minDist < _globalThreshold ? classification : "UNKNOWN") + " " /*+minDist.ToString()*/;
        
        }

        /// <summary>
        /// Retrieves a text represeantation of the _label and its associated _sequence
        /// For use in dispaying debug information and for saving to file
        /// </summary>
        /// <returns>A string containing all recorded gestures and their names</returns>
        public string RetrieveText()
        {
            string retStr = String.Empty;

            if (_sequences != null)
            {
                // Iterate through each gesture
                for (int gestureNum = 0; gestureNum < _sequences.Count; gestureNum++)
                {
                    // Echo the label
                    retStr += _labels[gestureNum] + "\r\n";

                    int frameNum = 0;

                    //Iterate through each frame of this gesture
                    foreach (double[] frame in ((ArrayList)_sequences[gestureNum]))
                    {
                        // Extract each double
                        foreach (double dub in (double[])frame)
                        {
                            retStr += dub + "\r\n";
                        }

                        // Signifies end of this double
                        retStr += "~\r\n";

                        frameNum++;
                    }

                    // Signifies end of this gesture
                    retStr += "----";
                    if (gestureNum < _sequences.Count - 1)
                    {
                        retStr += "\r\n";
                    }
                }
            }

            return retStr;
        }

        public ArrayList getSequence()
        {
            return _sequences;
        }

        /// <summary>
        /// Compute the min DTW distance between seq2 and all possible endings of seq1.
        /// </summary>
        /// <param name="seq1">The first array of sequences to compare</param>
        /// <param name="seq2">The second array of sequences to compare</param>
        /// <returns>The best match</returns>
        public double Dtw(ArrayList seq1, ArrayList seq2)
        {
            // Init
            var seq1R = new ArrayList(seq1);
            seq1R.Reverse();
            var seq2R = new ArrayList(seq2);
            seq2R.Reverse();
            var tab = new double[seq1R.Count + 1, seq2R.Count + 1];
            var slopeI = new int[seq1R.Count + 1, seq2R.Count + 1];
            var slopeJ = new int[seq1R.Count + 1, seq2R.Count + 1];

            for (int i = 0; i < seq1R.Count + 1; i++)
            {
                for (int j = 0; j < seq2R.Count + 1; j++)
                {
                    tab[i, j] = double.PositiveInfinity;
                    slopeI[i, j] = 0;
                    slopeJ[i, j] = 0;
                }
            }

            tab[0, 0] = 0;

            // Dynamic computation of the DTW matrix.
            for (int i = 1; i < seq1R.Count + 1; i++)
            {
                for (int j = 1; j < seq2R.Count + 1; j++)
                {
                    if (tab[i, j - 1] < tab[i - 1, j - 1] && tab[i, j - 1] < tab[i - 1, j] &&
                        slopeI[i, j - 1] < _maxSlope)
                    {
                        tab[i, j] = Dist2((double[]) seq1R[i - 1], (double[]) seq2R[j - 1]) + tab[i, j - 1];
                        slopeI[i, j] = slopeJ[i, j - 1] + 1;
                        slopeJ[i, j] = 0;
                    }
                    else if (tab[i - 1, j] < tab[i - 1, j - 1] && tab[i - 1, j] < tab[i, j - 1] &&
                             slopeJ[i - 1, j] < _maxSlope)
                    {
                        tab[i, j] = Dist2((double[]) seq1R[i - 1], (double[]) seq2R[j - 1]) + tab[i - 1, j];
                        slopeI[i, j] = 0;
                        slopeJ[i, j] = slopeJ[i - 1, j] + 1;
                    }
                    else
                    {
                        tab[i, j] = Dist2((double[]) seq1R[i - 1], (double[]) seq2R[j - 1]) + tab[i - 1, j - 1];
                        slopeI[i, j] = 0;
                        slopeJ[i, j] = 0;
                    }
                }
            }

            // Find best between seq2 and an ending (postfix) of seq1.
            double bestMatch = double.PositiveInfinity;
            for (int i = 1; i < (seq1R.Count + 1) - _minimumLength; i++)
            {
                if (tab[i, seq2R.Count] < bestMatch)
                {
                    bestMatch = tab[i, seq2R.Count];
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// Computes a 1-distance between two observations. (aka Manhattan distance).
        /// </summary>
        /// <param name="a">Point a (double)</param>
        /// <param name="b">Point b (double)</param>
        /// <returns>Manhattan distance between the two points</returns>
        private double Dist1(double[] a, double[] b)
        {
            double d = 0;
            for (int i = 0; i < _dimension; i++)
            {
                d += Math.Abs(a[i] - b[i]);
            }

            return d;
        }

        /// <summary>
        /// Computes a 2-distance between two observations. (aka Euclidian distance).
        /// </summary>
        /// <param name="a">Point a (double)</param>
        /// <param name="b">Point b (double)</param>
        /// <returns>Euclidian distance between the two points</returns>
        private double Dist2(double[] a, double[] b)
        {
            double d = 0;
            if (a.Length != b.Length)
            {
                return -1;
            }
            int dim = _dimension;
            if (_dimension > a.Length||(_dimension > b.Length))
            {
                dim = a.Length;
            }
            for (int i = 0; i < dim; i++)
            {
                d += Math.Pow(a[i] - b[i], 2);

            }

            return Math.Sqrt(d);
        }
    }
}