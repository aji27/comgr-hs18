using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    // Question: How should the BVH be implemented? As a bounding spehre or as a AABB? How is the space divided? 
    public class BVHNode
    {
        private IList<Sphere> _spheres;

        private Sphere _boundingBox;

        public BVHNode()
        {
            _spheres = new List<Sphere>();
        }

        public BVHNode(IEnumerable<Sphere> spheres)
            : this()
        {
            foreach (var sphere in spheres)
                _spheres.Add(sphere);
        }

        private void CalculateBoundingBox()
        {

        }
    }
}
