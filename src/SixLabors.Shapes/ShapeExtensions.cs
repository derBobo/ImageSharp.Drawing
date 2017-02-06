﻿// <copyright file="ShapeExtensions.cs" company="Scott Williams">
// Copyright (c) Scott Williams and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace SixLabors.Shapes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    /// <summary>
    /// Conveniance methods that can be applied to shapes and paths
    /// </summary>
    public static class ShapeExtensions
    {
        /// <summary>
        /// Creates a shape rotated by the specified radians around its center.
        /// </summary>
        /// <param name="shape">The shape to rotate.</param>
        /// <param name="radians">The radians to rotate the shape.</param>
        /// <returns>A <see cref="IShape"/> with a rotate transform applied.</returns>
        public static IShape Rotate(this IShape shape, float radians)
        {
            return shape.Transform(Matrix3x2.CreateRotation(radians, shape.Bounds.Center));
        }

        /// <summary>
        /// Creates a shape rotated by the specified degrees around its center.
        /// </summary>
        /// <param name="shape">The shape to rotate.</param>
        /// <param name="degrees">The degrees to rotate the shape.</param>
        /// <returns>A <see cref="IShape"/> with a rotate transform applied.</returns>
        public static IShape RotateDegree(this IShape shape, float degrees)
        {
            return shape.Rotate((float)(Math.PI * degrees / 180.0));
        }

        /// <summary>
        /// Creates a shape translated by the supplied postion
        /// </summary>
        /// <param name="shape">The shape to translate.</param>
        /// <param name="position">The translation position.</param>
        /// <returns>A <see cref="IShape"/> with a translate transform applied.</returns>
        public static IShape Translate(this IShape shape, Vector2 position)
        {
            return shape.Transform(Matrix3x2.CreateTranslation(position));
        }

        /// <summary>
        /// Creates a shape translated by the supplied postion
        /// </summary>
        /// <param name="shape">The shape to translate.</param>
        /// <param name="x">The amount to translate along the X axis.</param>
        /// <param name="y">The amount to translate along the Y axis.</param>
        /// <returns>A <see cref="IShape"/> with a translate transform applied.</returns>
        public static IShape Translate(this IShape shape, float x, float y)
        {
            return shape.Translate(new Vector2(x, y));
        }
    }
}
