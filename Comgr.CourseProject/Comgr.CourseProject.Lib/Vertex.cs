using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class Vertex
    {
        private Vector3 _position;
        private Vector3 _color;
        private int _screenWidth;
        private int _screenHeight;
        private Matrix4x4 _startMatrix;
        private Matrix4x4 _currentMatrix;

        private Vector4 _homogenousPosition;
        private Vector4 _homogenousColor;
        private Vector2 _screenPosition;

        public Vertex(Vector3 position, Vector3 color, int screenWidth, int screenHeight)
            : this(position, color, screenWidth, screenHeight, Matrix4x4.Identity)
        {
        }

        public Vertex(Vector3 position, Vector3 color, int screenWidth, int screenHeight, Matrix4x4 transform)
        {
            _position = position;
            _color = color;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _startMatrix = transform;
            _currentMatrix = _startMatrix;

            OnPropertyChanged();
        }

        public Vector3 Position => _position;

        public Vector4 HomogenousPosition => _homogenousPosition;

        public Vector3 Color => _color;

        public Vector4 HomogenousColor => _homogenousColor;

        public Vector2 ScreenPosition => _screenPosition;

        private void OnPropertyChanged()
        {
            _homogenousPosition = GetHomogenousPosition();
            _homogenousColor = GetHomogenousColor();
            _screenPosition = GetScreenPosition();
        }

        private Vector4 GetHomogenousPosition()
        {
            var v_homogenous = new Vector4(Position, w: 1);
            var v_transformed = Vector4.Transform(v_homogenous, _currentMatrix);
            return v_transformed;
        }

        private Vector4 GetHomogenousColor()
        {
            return new Vector4(Color / _homogenousPosition.W, 1 / _homogenousPosition.W);
        }

        private Vector2 GetScreenPosition()
        {
            var x = _screenWidth * _homogenousPosition.X / _homogenousPosition.Z + _screenWidth / 2;
            var y = _screenWidth * _homogenousPosition.Y / _homogenousPosition.Z + _screenHeight / 2;

            return new Vector2(x, y);
        }

        public void ApplyTransform(Matrix4x4 transform)
        {
            _currentMatrix = _startMatrix * transform;
            OnPropertyChanged();
        }
    }
}
