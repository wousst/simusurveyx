using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using SimuSurvey360.Instruments;

namespace SimuSurvey360
{
    class ViewController
    {
        #region Data Member
        //User Eyes
        private Vector3 _userEyes;
        private Vector3 _userEyes_bak;//for view mode
        private Vector3 _lookAtPosition; //the location for looking at
        private Vector3 _lookAtPosition_bak; //the location for looking at
        private bool _IsPerspectiveMode;
        private float _user_object_distance;
        private float _fov;
        private float _aspectRatio;
        private Viewport _viewport;
        private Matrix _view;
        private Matrix _projection;
        private Matrix _world;
        private float _XZRotationDegree;//-Z = 0
        private float _YZRotationDegree;//-Z = 0
        private Rectangle _Field; //for user activites
        #endregion

        public ViewController()
        {
            Initialization();
        }
        public ViewController(Viewport port, Rectangle field)
        {
            Initialization();
            _aspectRatio = (float)port.Width / (float)port.Height;
            _Field = field;
        }

        private void Initialization()
        {
            _user_object_distance = 80;
            _userEyes = new Vector3(0, 60f, 250);  // should be 51.403, seems too small
            _userEyes_bak = new Vector3(0, 20, 80);
            _IsPerspectiveMode = true;
            _fov = 1.0f;
            _lookAtPosition = _userEyes;
            _lookAtPosition.Z -= _user_object_distance;
            _XZRotationDegree = 0;
        }
        //For user specified position
        public void Initialization(Instrument instrument)
        {
            _user_object_distance = 80;
            _userEyes = instrument.WorldPosition;
            _userEyes.Y = instrument.Level;
            _IsPerspectiveMode = true;
            _fov = 1.0f;
            _lookAtPosition = _userEyes;
            _lookAtPosition.Z -= _user_object_distance;
            _XZRotationDegree = 0;
        }

        public void Update()
        {
            _view = Matrix.CreateLookAt(_userEyes, _lookAtPosition, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _aspectRatio, 5, 10000);
            _world = Matrix.Identity;

        }

        #region Get Set Functions
        public Vector3 UserEyes
        {
            set
            {
                _userEyes = value;
            }
            get
            { return _userEyes;}
        }

        public float UserObjectDistance
        {
            set
            { _user_object_distance = value;}
            get
            { return _user_object_distance;}
        }

        public float FOV
        {
            set{  _fov = value;}
            get
            {  return _fov;}
        }

        public float AspectRatio
        {
            get { return _aspectRatio; }
        }

        public Viewport ViewPort
        {
            set
            {
                _viewport = value;
                _aspectRatio = (float)_viewport.Width / (float)_viewport.Height;
            }
            get { return _viewport; }
        }
        public Matrix World
        {
            set { _world = value; }
            get { return _world; }
        }
        public Matrix View
        {
            set { _view = value; }
            get { return _view; }
        }
        public Matrix Projection
        {
            set { _projection = value; }
            get { return _projection; }
        }
        public float XZRotationValue
        {
            set { _XZRotationDegree = value; }
            get { return _XZRotationDegree; }
        }
        public float YZRotationValue
        {
            set { _YZRotationDegree = value; }
            get { return _YZRotationDegree; }
        }
        #endregion

        #region Move The Eyes of users
        public void MoveEyesUp(float distance)//y axis
        {
            Vector3 temp = _userEyes;
            temp.Y += distance;
            _userEyes = temp;
        }

        public void MoveEyesDown(float distance)//y axis
        {
            Vector3 temp = _userEyes;
            temp.Y -= distance;
            _userEyes = temp;
        }

        public void MoveEyesLeft(float distance)//x axis
        {
            Vector3 temp = _userEyes;
            temp.X += distance;
            if (Math.Abs(temp.X) < _Field.Width / 2)
            {
                _lookAtPosition.X += distance;
                _userEyes = temp;
            }
        }

        public void MoveEyesRight(float distance)//x axis
        {
            Vector3 temp = _userEyes;
            temp.X -= distance;
            _lookAtPosition.X += distance;
            _userEyes = temp;
        }

        public void UserMoveForwards(float distance)
        {
            Vector3 location = UserEyes;
            float Cos = (float)(Math.Cos(MathHelper.ToRadians(_XZRotationDegree)));
            float Sin = (float)(Math.Sin(MathHelper.ToRadians(_XZRotationDegree)));
            location.Z -= distance * Cos;
            location.X -= distance * Sin;
            if (Math.Abs(location.X) < _Field.Width / 2 && Math.Abs(location.Z) < _Field.Width/2)
            {
                _userEyes.X = location.X;
                _userEyes.Z = location.Z;
                _lookAtPosition.Z -= distance * Cos;
                _lookAtPosition.X -= distance * Sin;
            }
        }

        public void UserMoveBackwards(float distance)
        {
            Vector3 location = UserEyes;
            float Cos = (float)(Math.Cos(MathHelper.ToRadians(_XZRotationDegree)));
            float Sin = (float)(Math.Sin(MathHelper.ToRadians(_XZRotationDegree)));
            location.Z += distance * Cos;
            location.X += distance * Sin;
            _userEyes.X = location.X;
            _userEyes.Z = location.Z;
            _lookAtPosition.Z += distance * Cos;
            _lookAtPosition.X += distance * Sin;
        }

        public void UserTurnLeft(float degree)
        {
            _XZRotationDegree += degree;
            Vector3 location = UserEyes;
            float Cos = (float)(Math.Cos(MathHelper.ToRadians(_XZRotationDegree)));
            float Sin = (float)(Math.Sin(MathHelper.ToRadians(_XZRotationDegree)));
            location.Z -= (_user_object_distance) * Cos;
            location.X -= (_user_object_distance)  * Sin;
            _lookAtPosition.X = location.X;
            _lookAtPosition.Z = location.Z;
        }

        public void UserTurnUp(float degree)
        {
            _YZRotationDegree += degree;
            Vector3 location = UserEyes;
            float Cos = (float)(Math.Cos(MathHelper.ToRadians(_YZRotationDegree)));
            float Sin = (float)(Math.Sin(MathHelper.ToRadians(_YZRotationDegree)));
            location.Z -= (_user_object_distance) * Cos;
            location.Y += (_user_object_distance) * Sin;
            _lookAtPosition.Y = location.Y;
            _lookAtPosition.Z = location.Z;
        }

        public void UserTurnLeftWithValue(float degree)
        {
            _XZRotationDegree = degree;
            Vector3 location = UserEyes;
            float Cos = (float)(Math.Cos(MathHelper.ToRadians(_XZRotationDegree)));
            float Sin = (float)(Math.Sin(MathHelper.ToRadians(_XZRotationDegree)));
            location.Z -= (_user_object_distance) * Cos;
            location.X -= (_user_object_distance) * Sin;
            _lookAtPosition.X = location.X;
            _lookAtPosition.Z = location.Z;
        }

        public void UserTurnUpWithValue(float degree)
        {
            _YZRotationDegree = degree;
            Vector3 location = UserEyes;
            float Cos = (float)(Math.Cos(MathHelper.ToRadians(_YZRotationDegree)));
            float Sin = (float)(Math.Sin(MathHelper.ToRadians(_YZRotationDegree)));
            location.Z -= (_user_object_distance) * Cos;
            location.Y += (_user_object_distance) * Sin;
            _lookAtPosition.Y = location.Y;
            _lookAtPosition.Z = location.Z;
        }

        public void ChangeToTopMode()
        {
            if (_userEyes != null && _lookAtPosition != null)
            {
                if (_IsPerspectiveMode == true)
                {
                    _userEyes_bak = _userEyes;
                    _lookAtPosition_bak = _lookAtPosition;
                }
                _IsPerspectiveMode = false;
                _userEyes = new Vector3(0, 0, 0);
                _lookAtPosition = new Vector3(0, 0, 0);
                _userEyes.Y += _user_object_distance *3;
                _userEyes.Z += 0.01f;
            }
        }
        public void ChangeToFrontMode()
        {
            if (_userEyes != null && _lookAtPosition != null)
            {
                if (_IsPerspectiveMode == true)
                {
                    _userEyes_bak = _userEyes;
                    _lookAtPosition_bak = _lookAtPosition;
                }
                _IsPerspectiveMode = false;
                _userEyes = new Vector3(0, 0, 0);
                _lookAtPosition = new Vector3(0, 0, 0);
                _userEyes.Z += _user_object_distance * 3;
                _userEyes.Y += 0.0000001f; //can't set to zero or the martix will be an error
            }
        }
        public void ChangeToLeftMode()
        {
            if (_userEyes != null && _lookAtPosition != null)
            {
                if (_IsPerspectiveMode == true)
                {
                    _userEyes_bak = _userEyes;
                    _lookAtPosition_bak = _lookAtPosition;
                }
                _IsPerspectiveMode = false;
                _userEyes = new Vector3(0, 0, 0);
                _lookAtPosition = new Vector3(0, 0, 0);
                _userEyes.X += _user_object_distance * 2;
                _userEyes.Z += 0.0000001f; //can't set to zero or the martix will be an error
            }
        }
        public void ChangeToPerspectiveMode()
        {
            if (_userEyes != null && _lookAtPosition != null)
            {
                _userEyes = _userEyes_bak;
                _lookAtPosition = _lookAtPosition_bak;
            }
            _IsPerspectiveMode = true;
        }
        #endregion

       
    }
}
