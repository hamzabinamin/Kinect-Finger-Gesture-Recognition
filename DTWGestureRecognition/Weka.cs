using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DTWGestureRecognition
{
    class Weka
    {
        public interface IClassifier
        {
            int ClassifyInstance(double[] atribute, out double[] sir_prcente);
            int InitializeClassifier();
        };

        public class BayesNaive : IClassifier
        {
            private weka.classifiers.Classifier m_cl;
            private weka.core.Instance testInstance;
            private weka.core.Instances dataSet;


            public int ClassifyInstance(double[] attributes, out double[] percentages)
            {
                double classificationResult = 1.0;
                testInstance.setDataset(dataSet);
                testInstance.setClassMissing();
                dataSet.add(testInstance);
                for (int i = 0; i < attributes.Length; i++)
                {
                    testInstance.setValue(i, attributes[i]);
                }       
                classificationResult = m_cl.classifyInstance(testInstance);
                dataSet.delete(0);
                percentages = m_cl.distributionForInstance(testInstance);
                return ((int)classificationResult);
            }
            public int InitializeClassifier()
            { 
                return 1;
            }
            public int InitializeClassifier(string[] atributes,string[] gestures,string classAttribute, string modelLocation)
            {
                java.io.ObjectInputStream ois = new java.io.ObjectInputStream(new java.io.FileInputStream(modelLocation));
                  

                m_cl = (weka.classifiers.Classifier)ois.readObject();

                //Declare the feature vector
                weka.core.FastVector fvWekaFeatureVector = new weka.core.FastVector(atributes.Length+1);
                for (int i = 0; i < atributes.Length; i++)
                {
                    weka.core.Attribute aux = new weka.core.Attribute(atributes[i]);
                    fvWekaFeatureVector.addElement(aux);
                }


                //Declare the class weka.core.Attribute along with its values
                weka.core.FastVector fvClassValues = new weka.core.FastVector(gestures.Length);
                for (int i = 0; i < gestures.Length; i++)
                {
                    weka.core.Attribute z1 = new weka.core.Attribute(atributes[i]);
                    fvClassValues.addElement(gestures[i]);
                }
                //fvClassValues.addElement("yes");
                //fvClassValues.addElement("no");

                weka.core.Attribute ClassAttribute = new weka.core.Attribute(classAttribute, fvClassValues);

                fvWekaFeatureVector.addElement(ClassAttribute);

                dataSet = new weka.core.Instances("TestRel", fvWekaFeatureVector, 10);
                dataSet.setClassIndex(atributes.Length);

                testInstance = new weka.core.Instance(atributes.Length+1);

                return 1;
            }

        }
    }
}
