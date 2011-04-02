﻿using BEPUphysics.CollisionShapes.ConvexShapes;
using Microsoft.Xna.Framework;
using System;
using BEPUphysics.PositionUpdating;
using BEPUphysics.Settings;

namespace BEPUphysics.Collidables.MobileCollidables
{
    ///<summary>
    /// Collidable with a convex shape.
    ///</summary>
    public abstract class ConvexCollidable : EntityCollidable
    {

        protected ConvexCollidable(ConvexShape shape)
            : base(shape)
        {

        }

        ///<summary>
        /// Gets the shape of the collidable.
        ///</summary>
        public new ConvexShape Shape
        {
            get
            {
                return base.Shape as ConvexShape;
            }
        }





    }

    ///<summary>
    /// Collidable with a convex shape of a particular type.
    ///</summary>
    ///<typeparam name="T">ConvexShape type.</typeparam>
    public class ConvexCollidable<T> : ConvexCollidable where T : ConvexShape
    {
        ///<summary>
        /// Gets the shape of the collidable.
        ///</summary>
        public new T Shape
        {
            get
            {
                return base.Shape as T;
            }
        }

        ///<summary>
        /// Constructs a new convex collidable.
        ///</summary>
        ///<param name="shape">Shape to use in the collidable.</param>
        public ConvexCollidable(T shape)
            : base(shape)
        {

        }


        /// <summary>
        /// Tests a ray against the entry.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length, in units of the ray's direction's length, to test.</param>
        /// <param name="rayHit">Hit location of the ray on the entry, if any.</param>
        /// <returns>Whether or not the ray hit the entry.</returns>
        public override bool RayCast(Ray ray, float maximumLength, out RayHit rayHit)
        {
            return Shape.RayTest(ref ray, ref worldTransform, maximumLength, out rayHit);
        }



        protected internal override void UpdateBoundingBoxInternal(float dt)
        {
            Shape.GetBoundingBox(ref worldTransform, out boundingBox);

            ExpandBoundingBox(ref boundingBox, dt);
        }



    }
}
