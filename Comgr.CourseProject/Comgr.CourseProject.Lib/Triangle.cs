﻿using System.Numerics;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class Triangle
    {
        private Vector2 _a;
        private Vector2 _b;
        private Vector2 _c;

        private Matrix2x2 _inverse;
        
        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            _a = a;
            _b = b;
            _c = c;

            var AB = _b - _a;
            var AC = _c - _a;

            var A = new Matrix2x2(AB.X, AB.Y, AC.X, AC.Y);
            _inverse = A.Inverse();                        
        }

        public Vector3 CalcColor(float x, float y)
        {
            var p = new Vector2(x, y);
            var AP = p - _a;
            var vec = _inverse * AP;
            var u = vec.X;
            var v = vec.Y;
            bool drawPoint = (u >= 0 && v >= 0 && (u + v) < 1);

            if (drawPoint)
            {
                return Conversions.FromColor(Colors.LightSkyBlue);
            }

            return Vector3.Zero;
        }        
    }
}