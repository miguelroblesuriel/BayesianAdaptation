// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

class Program
{
    private static float distance_to_elbow; //in cm
    private static float arm_lenght; //in cm
    private static float arm_diameter; //in cm
    private static float distance_between_pads = 3; //in cm

    static float[] ObtainMotionModel(float previous_angle, float actual_angle, float distance_to_elbow, float arm_diameter)
    {
        float[] motion_matrix = new float[15];
        for (int i = 0; i<4; i++)
        {
            float total_probability = 1 / 15 * i;
            float corrected_angle = (actual_angle - previous_angle) * (distance_to_elbow+(3-i)*2);
            float distance = (corrected_angle / 180) * float.Pi * (arm_diameter / 2);
            if (i != 3)
            {
                if (distance > 2)
                {
                    motion_matrix[i] = total_probability/6*1;
                    motion_matrix[i + 4] = total_probability / 6 * 1;
                    motion_matrix[i + 8] = total_probability / 6 * 2;
                    motion_matrix[i + 12] = total_probability / 6 * 2;
                }
                else if(distance < -2)
                {
                    motion_matrix[i] = total_probability / 6 * 2;
                    motion_matrix[i + 4] = total_probability / 6 * 2;
                    motion_matrix[i + 8] = total_probability / 6 * 1;
                    motion_matrix[i + 12] = total_probability / 6 * 1;
                }
                else
                {
                    motion_matrix[i] = total_probability;
                    motion_matrix[i + 4] = total_probability;
                    motion_matrix[i + 8] = total_probability;
                    motion_matrix[i + 12] = total_probability;
                }
            }
            else
            {
                if (distance > 2)
                {
                    motion_matrix[i] = total_probability / 5 * 1;
                    motion_matrix[i + 4] = total_probability / 5 * 1;
                    motion_matrix[i + 8] = total_probability / 5 * 3;
                }
                else if (distance < -2)
                {
                    motion_matrix[i] = total_probability / 5 * 2;
                    motion_matrix[i + 4] = total_probability / 5 * 2;
                    motion_matrix[i + 8] = total_probability / 5 * 1;
                }
                else
                {
                    motion_matrix[i] = total_probability;
                    motion_matrix[i + 4] = total_probability;
                    motion_matrix[i + 8] = total_probability;
                }
            }
            
        }
        return motion_matrix;
    }
    static float[] BayesianPrediction(float[] previus_probabilities, float previous_angle, float actual_angle)
    {
        float[] new_probabilities = new float[15];
        float[] predictions = new float[15];
        float[] corrections = new float[15];
        float sum_of_probabilities = 0;
        float[] motion_matrix = ObtainMotionModel(previous_angle, actual_angle, distance_to_elbow, arm_diameter);
        for (int i =0; i< 15; i++)
        {
            predictions[i] = motion_matrix[i] * previus_probabilities[i];
            corrections[i] = predictions[i]*GetProbabilityFromAngle(i,actual_angle);
            sum_of_probabilities = sum_of_probabilities+ corrections[i];
        }
        for(int i = 0; i<15, i++)
        {
            new_probabilities[i]= corrections[i]/sum_of_probabilities
        }
        return new_probabilities;
    }

    static void Main(string[] args)
    {
        // Function call
        float[] probabilities = new float[15];
        float actual_angle;
        float previous_angle = 0;
        for(int i = 0; i<15; i++)
        {
            probabilities[i] = 1 / 15;
        }
        while (true)
        {
            actual_angle = getActualAngle();
            probabilities = BayesianPrediction(probabilities, previous_angle, actual_angle);
            WriteProbabilities(probabilities);
            previous_angle = actual_angle;
        }
    }
}