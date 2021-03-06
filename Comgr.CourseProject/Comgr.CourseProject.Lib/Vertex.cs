﻿using System;
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
        private Vector3 _homogenousTexturePosition;

        private Vector2 _texturePosition;

        private Vector3 _startNormal;
        private Vector3 _currentNormal;

        public Vertex(Vector3 position, Vector3 color, int screenWidth, int screenHeight, Vector2 texturePosition)
            : this(position, color, screenWidth, screenHeight, texturePosition, Matrix4x4.Identity)
        {
        }

        public Vertex(Vector3 position, Vector3 color, int screenWidth, int screenHeight, Vector2 texturePosition, Matrix4x4 transform)
        {
            _position = position;
            _color = color;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;

            _texturePosition = texturePosition;

            _startMatrix = transform;
            _currentMatrix = _startMatrix;

            OnPropertyChanged();
        }                       
        
        public Vector4 HomogenousPosition => _homogenousPosition;
                
        public Vector4 HomogenousColor => _homogenousColor;

        public Vector2 ScreenPosition => _screenPosition;

        public Vector3 HomogenousTexturePosition => _homogenousTexturePosition;

        public Vector3 Normal
        {
            get
            {
                return _currentNormal;
            }
            set
            {
                _startNormal = value;
                _currentNormal = Vector3.TransformNormal(_startNormal, _currentMatrix);
            }
        }

        private void OnPropertyChanged()
        {
            _homogenousPosition = GetHomogenousPosition();
            _homogenousColor = GetHomogenousColor();
            _screenPosition = GetScreenPosition();
            _homogenousTexturePosition = GetHomogenousTexturePosition();
        }

        private Vector4 GetHomogenousPosition()
        {
            var v_homogenous = new Vector4(_position, w: 1);
            var v_transformed = Vector4.Transform(v_homogenous, _currentMatrix);
            return v_transformed;
        }

        private Vector4 GetHomogenousColor()
        {
            return new Vector4(_color / _homogenousPosition.W, 1f / _homogenousPosition.W);
        }

        private Vector2 GetScreenPosition()
        {
            var homogenousPosition = _homogenousPosition.HomogenousNormalize();
            var x = _screenWidth * homogenousPosition.X / homogenousPosition.Z + _screenWidth / 2;
            var y = _screenWidth * homogenousPosition.Y / homogenousPosition.Z + _screenHeight / 2;

            return new Vector2(x, y);
        }

        private Vector3 GetHomogenousTexturePosition()
        {
            var homogenousPosition = _homogenousPosition.HomogenousNormalize();
            return new Vector3(_texturePosition / homogenousPosition.Z, 1f / homogenousPosition.Z);
        }

        public void ApplyTransform(Matrix4x4 transform)
        {
            _currentMatrix = _startMatrix * transform;
            _currentNormal = Vector3.TransformNormal(_startNormal, transform);

            OnPropertyChanged();
        }
    }
}
