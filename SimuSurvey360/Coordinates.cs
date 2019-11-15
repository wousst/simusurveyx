using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace SimuSurvey360
{
    class Coordinates
    {

        #region DataMember
        GraphicsDevice _gd;

        Rectangle _Field;
        float _axisLength = 500f; //a block is 10 unit
        VertexPositionColor[] _subXVertices;
        VertexPositionColor[] _subZVertices;

        //Other axis
        int _XAxisCount;   //How many lines are needed in one dimension
        int _ZAxisCount;   //How many lines are needed in one dimension
        float _margin;    //Margin between two lines

        //Background
        int _Points;
        VertexPositionColor[] _PointListF; //Front
        VertexPositionColor[] _PointListB;//Back
        VertexPositionColor[] _PointListL;//Left
        VertexPositionColor[] _PointListR;//Right
        VertexPositionColor[] _PointListT;//Top
        VertexPositionColor[] _PointListO;//Floor
        short[] _TriangleStripIndices;

        Matrix _World;
        Matrix _View;
        Matrix _Projection;
        #endregion

        public Coordinates()
        {

        }

        public Coordinates(GraphicsDevice gd,Rectangle field)
        {
            _gd = gd;
            _Field = field;
            _axisLength = field.Width;
        }

       public void LoadContent()
        {
            _XAxisCount =(int) (_axisLength/10) +2;
            _ZAxisCount =(int) (_axisLength/10) +2;
            _margin = 10;
            _subXVertices = new VertexPositionColor[_XAxisCount * 2];
            _subZVertices = new VertexPositionColor[_ZAxisCount * 2];

            int  index = 0;
            float halfSide = _axisLength / 2;

            for (int i = 0; i < _XAxisCount; i++)
            {
                if(i%2==0)
                    _subXVertices[index] = new VertexPositionColor(new Vector3(-(i / 2) * _margin, 0.0f, -halfSide), Color.White);
                else
                    _subXVertices[index] = new VertexPositionColor(new Vector3(-(i / 2) * _margin, 0.0f, halfSide), Color.White);
                index++;
            }

            for (int i = 0; i < _XAxisCount; i++)
            {
                if (i % 2 == 0)
                    _subXVertices[index] = new VertexPositionColor(new Vector3((i / 2) * _margin, 0.0f, -halfSide), Color.White);
                else
                    _subXVertices[index] = new VertexPositionColor(new Vector3((i / 2) * _margin, 0.0f, halfSide), Color.White);
                index++;
            }
           
            index = 0;
            for (int i = 0; i < _ZAxisCount; i++)
            {
                if (i % 2 == 0)
                    _subZVertices[index] = new VertexPositionColor(new Vector3(-halfSide, 0.0f, -(i / 2) * _margin), Color.White);
                else
                    _subZVertices[index] = new VertexPositionColor(new Vector3(halfSide, 0.0f, -(i / 2) * _margin), Color.White);
                index++;
            }

            for (int i = 0; i < _ZAxisCount; i++)
            {
                if (i % 2 == 0)
                    _subZVertices[index] = new VertexPositionColor(new Vector3(-halfSide, 0.0f, (i / 2) * _margin), Color.White);
                else
                    _subZVertices[index] = new VertexPositionColor(new Vector3(halfSide, 0.0f, (i / 2) * _margin), Color.White);
                index++;
            }

           //Background
            _Points = 4;
            // Initialize an array of indices of type short.
            _TriangleStripIndices = new short[_Points];
            // Populate the array with references to indices in the vertex buffer.
            for (int i = 0; i < _Points; i++)
                _TriangleStripIndices[i] = (short)i;
           // Front 
           _PointListF = new VertexPositionColor[_Points];
           _PointListF[0] = new VertexPositionColor(new Vector3(-halfSide, 0, -halfSide), Color.Black);
           _PointListF[1] = new VertexPositionColor(new Vector3(-halfSide, halfSide, -halfSide), Color.Black);
           _PointListF[2] = new VertexPositionColor(new Vector3(halfSide, 0, -halfSide), Color.Black);
           _PointListF[3] = new VertexPositionColor(new Vector3(halfSide, halfSide, -halfSide), Color.Black);

           // Floor
           _PointListO = new VertexPositionColor[_Points];
           _PointListO[0] = new VertexPositionColor(new Vector3(-halfSide, 0, -halfSide), Color.Black);
           _PointListO[1] = new VertexPositionColor(new Vector3(halfSide, 0, -halfSide), Color.Black);
           _PointListO[2] = new VertexPositionColor(new Vector3(-halfSide, 0, halfSide), Color.Black);
           _PointListO[3] = new VertexPositionColor(new Vector3(halfSide, 0, halfSide), Color.Black);

           // Ceiling
           _PointListT = new VertexPositionColor[_Points];
           _PointListT[0] = new VertexPositionColor(new Vector3(-halfSide, halfSide, -halfSide), Color.Black);
           _PointListT[1] = new VertexPositionColor(new Vector3(-halfSide, halfSide, halfSide), Color.Black);
           _PointListT[2] = new VertexPositionColor(new Vector3(halfSide, halfSide, -halfSide), Color.Black);
           _PointListT[3] = new VertexPositionColor(new Vector3(halfSide, halfSide, halfSide), Color.Black);

           // Back
           _PointListB = new VertexPositionColor[_Points];
           _PointListB[0] = new VertexPositionColor(new Vector3(-halfSide, 0, halfSide), Color.Black);
           _PointListB[1] = new VertexPositionColor(new Vector3(halfSide, 0, halfSide), Color.Black);
           _PointListB[2] = new VertexPositionColor(new Vector3(-halfSide, halfSide, halfSide), Color.Black);
           _PointListB[3] = new VertexPositionColor(new Vector3(halfSide, halfSide, halfSide), Color.Black);

           // Left
           _PointListL = new VertexPositionColor[_Points];
           _PointListL[0] = new VertexPositionColor(new Vector3(-halfSide, 0, halfSide), Color.Black);
           _PointListL[1] = new VertexPositionColor(new Vector3(-halfSide, halfSide, halfSide), Color.Black);
           _PointListL[2] = new VertexPositionColor(new Vector3(-halfSide, 0, -halfSide), Color.Black);
           _PointListL[3] = new VertexPositionColor(new Vector3(-halfSide, halfSide, -halfSide), Color.Black);

           //Right
           _PointListR = new VertexPositionColor[_Points];
           _PointListR[0] = new VertexPositionColor(new Vector3(halfSide, 0, halfSide), Color.Black);
           _PointListR[1] = new VertexPositionColor(new Vector3(halfSide, 0, -halfSide), Color.Black);
           _PointListR[2] = new VertexPositionColor(new Vector3(halfSide, halfSide, halfSide), Color.Black);
           _PointListR[3] = new VertexPositionColor(new Vector3(halfSide, halfSide, -halfSide), Color.Black);

        }

       public void Update(Matrix world, Matrix view, Matrix projection)
       {
           _World = world;
           _View = view;
           _Projection = projection;
       }

       public void Draw()
       {
           if (_gd == null)
               return;

           _gd.VertexDeclaration = new VertexDeclaration(_gd, VertexPositionColor.VertexElements);

           BasicEffect effect = new BasicEffect(_gd, null);

           effect.VertexColorEnabled = true;
           effect.World = _World;
           effect.View = _View;
           effect.Projection = _Projection;
           effect.Begin();

           foreach (EffectPass currentPass in effect.CurrentTechnique.Passes)
           {
               currentPass.Begin();
               //background
               DrawRectangle(_PointListO, Color.Gray);
               DrawRectangle(_PointListT, Color.LightGray);
               DrawRectangle(_PointListF, Color.LightGray);
               DrawRectangle(_PointListB, Color.LightGray);
               DrawRectangle(_PointListL, Color.LightGray);
               DrawRectangle(_PointListR, Color.LightGray);
               //grid
               _gd.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, _subXVertices, 0, _XAxisCount);
               _gd.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, _subZVertices, 0, _ZAxisCount);

               
               currentPass.End();
           }
           effect.End();
           
       }
   
            

        private void DrawRectangle(VertexPositionColor[] pointlist, Color color)
        {
            for (int i = 0; i < pointlist.Length; i++)
                pointlist[i].Color = color;

        _gd.DrawUserIndexedPrimitives<VertexPositionColor>(
            PrimitiveType.TriangleStrip,
            pointlist,
            0,  // vertex buffer offset to add to each element of the index buffer
            _Points,  // number of vertices to draw
            _TriangleStripIndices,
            0,  // first index element to read
            _Points-2   // number of primitives to draw
        );
        for (int i = 0; i < pointlist.Length; i++)
            pointlist[i].Color = Color.White;
        }
        
    }
}
