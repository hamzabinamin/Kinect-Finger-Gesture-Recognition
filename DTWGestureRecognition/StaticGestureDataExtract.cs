using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DTWGestureRecognition
{
    using System;
    using System.Collections;
    class StaticGestureDataExtract
    {
       
        /// <summary>
        /// The gesture names. Index matches that of the sequences array in _sequences
        /// </summary>
        private readonly ArrayList _labels = new ArrayList();

        /// <summary>
        /// The recorded gesture sequences
        /// </summary>
        private readonly ArrayList _sequences = new ArrayList();

        public StaticGestureDataExtract()
        {
            _labels = new ArrayList();
            _sequences = new ArrayList();
        }
        /// <summary>
        /// Add a seqence with a label to the known sequences library.
        /// The gesture MUST start on the first observation of the sequence and end on the last one.
        /// Sequences may have different lengths.
        /// </summary>
        /// <param name="seq">The sequence</param>
        /// <param name="lab">Sequence name</param>
        /// /// <param name="lab">maxPos maximum number of entries for a label</param>
        public  void AddOrUpdate(ArrayList seq, string lab, int maxPos)
        {
            // First we check whether there is already a recording for this label. If so overwrite it, otherwise add a new entry
            int firstIndex = -1;
            int number = 0;


            for (int i = 0; i < _labels.Count; i++)
            {
                if ((string)_labels[i] == lab)
                {
                    if (firstIndex == -1)
                    {
                        firstIndex = i;
                    };
                    number = number + 1;
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
        /// Retrieves a text represeantation of the _label and its associated _sequence
        /// For use in dispaying debug information and for saving to file
        /// </summary>
        /// <returns>A string containing all recorded gestures and their names</returns>
        public string RetrieveText(string[] atributes , string[] staticGestures, string gestureType)
        {
            string retStr = String.Empty;

            retStr = "@relation " + gestureType + "\r\n";
            retStr += "\r\n";
            for (int i = 0; i < atributes.Length; i++)
            {
                retStr += "@attribute " + atributes[i] + " real \r\n";
            }
            retStr += "@attribute gesture {" ;
            for (int i = 0; i < staticGestures.Length - 1; i++)
            {
                retStr += staticGestures[i] + ", ";
            }
            retStr += staticGestures[staticGestures.Length - 1] + "} \r\n";
            retStr += "\r\n";
            retStr += "@data \r\n";
            if (_sequences != null)
            {
                // Iterate through each gesture
                for (int gestureNum = 0; gestureNum < _sequences.Count; gestureNum++)
                {
                    int frameNum = 0;

                    //Iterate through each frame of this gesture
                    foreach (double[] frame in ((ArrayList)_sequences[gestureNum]))
                    {
                        // Extract each double
                        foreach (double dub in (double[])frame)
                        {

                            retStr += ((int)(dub*10000)) + ", ";
                        }

                        // Signifies end of this double
                        retStr +=  _labels[gestureNum]+"\r\n";

                        frameNum++;
                    }

                    // Signifies end of this gesture
                    //retStr += "----";
                    if (gestureNum < _sequences.Count - 1)
                    {
                      //  retStr += "\r\n";
                    }
                }
            }

            return retStr;
        }
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
    }

}
