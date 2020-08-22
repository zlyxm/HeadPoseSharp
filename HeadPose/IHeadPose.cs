using OpenCvSharp;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadPoseSharp
{
    public interface IHeadPose
    {
        Angles GetAnglesAndPoints(Mat<Point2d> points, int width, int height);

    }
    public class Emgu_Dlib_OpenCv : IHeadPose
    {
        public Mat<Point3d> Model_points => new Mat<Point3d>(1, 6,
                new Point3d[] {
                    new Point3d(0.0f, 0.0f, 0.0f),
                    new Point3d(0.0f, -330.0f, -65.0f),
                    new Point3d(-225.0f, 170.0f, -135.0f),
                    new Point3d(225.0f, 170.0f, -135.0f),
                    new Point3d(-150.0f, -150.0f, -125.0f),
                    new Point3d(150.0f, -150.0f, -125.0f)
                });

        public Angles GetAnglesAndPoints(Mat<Point2d> points, int width, int height)
        {
            var cameraMatrix = GetCameraMatrix(width, height);

            Mat rotation = new Mat<double>();
            Mat translation = new Mat<double>();
            Mat coeffs = new Mat<double>(4, 1);
            Cv2.SolvePnP(Model_points, points, cameraMatrix, coeffs, rotation, translation);
            var euler = GetEulerMatrix(rotation);
            var pitch = 180 * euler.At<double>(0, 1) / Math.PI;
            pitch = Math.Sign(pitch) * 180 - pitch;

            var roll = 180 * euler.At<double>(0, 0) / Math.PI;
            var yaw = 180 * euler.At<double>(0, 2) / Math.PI;
            return new Angles() { Pitch = pitch, Roll = roll, Yaw = yaw };
        }
        public static Mat<double> GetCameraMatrix(int width, int height)
        {
            return new Mat<double>(3, 3,
                new double[] {
                    width, 0,     width / 2,
                    0,     width, height / 2,
                    0,     0,     1
                });
        }

        /// <summary>
        /// Find the Euler matrix from the output of SolvePnP.
        /// </summary>
        /// <param name="rotation">The rotation matrix returned by SolvePnp.</param>
        /// <returns>The Euler matrix containing pitch, roll, and yaw angles.</returns>
        public static Mat<double> GetEulerMatrix(Mat rotation)
        {
            // convert the 1x3 rotation vector to a full 3x3 matrix
            var r = new Mat<double>(3, 3);
            Cv2.Rodrigues(rotation, r);

            // set up some shortcuts to rotation matrix
            double m00 = r.At<double>(0, 0);
            double m01 = r.At<double>(0, 1);
            double m02 = r.At<double>(0, 2);
            double m10 = r.At<double>(1, 0);
            double m11 = r.At<double>(1, 1);
            double m12 = r.At<double>(1, 2);
            double m20 = r.At<double>(2, 0);
            double m21 = r.At<double>(2, 1);
            double m22 = r.At<double>(2, 2);

            // set up output variables
            Angles euler_out = new Angles();
            Angles euler_out2 = new Angles();

            if (Math.Abs(m20) >= 1)
            {
                euler_out.Yaw = 0;
                euler_out2.Yaw = 0;

                // From difference of angles formula
                if (m20 < 0)  //gimbal locked down
                {
                    double delta = Math.Atan2(m01, m02);
                    euler_out.Pitch = Math.PI / 2f;
                    euler_out2.Pitch = Math.PI / 2f;
                    euler_out.Roll = delta;
                    euler_out2.Roll = delta;
                }
                else // gimbal locked up
                {
                    double delta = Math.Atan2(-m01, -m02);
                    euler_out.Pitch = -Math.PI / 2f;
                    euler_out2.Pitch = -Math.PI / 2f;
                    euler_out.Roll = delta;
                    euler_out2.Roll = delta;
                }
            }
            else
            {
                euler_out.Pitch = -Math.Asin(m20);
                euler_out2.Pitch = Math.PI - euler_out.Pitch;

                euler_out.Roll = Math.Atan2(m21 / Math.Cos(euler_out.Pitch), m22 / Math.Cos(euler_out.Pitch));
                euler_out2.Roll = Math.Atan2(m21 / Math.Cos(euler_out2.Pitch), m22 / Math.Cos(euler_out2.Pitch));

                euler_out.Yaw = Math.Atan2(m10 / Math.Cos(euler_out.Pitch), m00 / Math.Cos(euler_out.Pitch));
                euler_out2.Yaw = Math.Atan2(m10 / Math.Cos(euler_out2.Pitch), m00 / Math.Cos(euler_out2.Pitch));
            }

            // return result
            return new Mat<double>(1, 3, new double[] { euler_out.Yaw, euler_out.Roll, euler_out.Pitch });
        }


    }

}
