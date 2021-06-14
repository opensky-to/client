// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rect.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.Native.PInvoke.Structs
{
    using System;
    using System.Runtime.InteropServices;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Rectangle struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect : IEquatable<Rect>
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Empty Rect structure.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly Rect Empty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Left coordinate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int left;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Top coordinate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int top;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Right coordinate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int right;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Bottom coordinate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int bottom;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Rect"/> struct. Value constructor.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="left">
        /// Left coordinate.
        /// </param>
        /// <param name="top">
        /// Top coordinate.
        /// </param>
        /// <param name="right">
        /// Right coordinate.
        /// </param>
        /// <param name="bottom">
        /// Bottom coordinate.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Rect(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Rect"/> struct. Copying constructor.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="rcSrc">
        /// Source rectangle to copy.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Rect(Rect rcSrc)
        {
            this.left = rcSrc.left;
            this.top = rcSrc.top;
            this.right = rcSrc.right;
            this.bottom = rcSrc.bottom;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the width of the Rect.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Width => Math.Abs(this.right - this.left);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the height of the Rect.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Height => this.bottom - this.top;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the Rect is empty.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsEmpty => this.left >= this.right || this.top >= this.bottom;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determine if 2 Rect are equal (deep compare).
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="rect1">
        /// First Rect.
        /// </param>
        /// <param name="rect2">
        /// Second Rect.
        /// </param>
        /// <returns>
        /// True if the RECTS are equal, false otherwise.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static bool operator ==(Rect rect1, Rect rect2)
        {
            return rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right
                   && rect1.bottom == rect2.bottom;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determine if 2 Rect are different(deep compare).
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="rect1">
        /// First Rect.
        /// </param>
        /// <param name="rect2">
        /// Second Rect.
        /// </param>
        /// <returns>
        /// True if the RECTS are not equals, false otherwise.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static bool operator !=(Rect rect1, Rect rect2)
        {
            return !(rect1 == rect2);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Return a user friendly representation of this struct.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <returns>
        /// A user friendly representation of this struct.
        /// </returns>
        /// <seealso cref="M:System.ValueType.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            if (this == Empty)
            {
                return "Rect {Empty}";
            }

            return "Rect { left : " + this.left + " / top : " + this.top + " / right : " + this.right + " / bottom : "
                   + this.bottom + " }";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determine if 2 Rect are equal (deep compare).
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="obj">
        /// The object to compare to.
        /// </param>
        /// <returns>
        /// True if the specified object equals this Rect, false otherwise.
        /// </returns>
        /// <seealso cref="M:System.ValueType.Equals(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (!(obj is System.Windows.Rect))
            {
                return false;
            }

            // ReSharper disable PossibleInvalidCastException
            return this == (Rect)obj;

            // ReSharper restore PossibleInvalidCastException
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Return the HashCode for this struct (not guaranteed to be unique).
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <returns>
        /// The numerical HashCode for this struct.
        /// </returns>
        /// <seealso cref="M:System.ValueType.GetHashCode()"/>
        /// -------------------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return 13
                   * (this.left.GetHashCode() + this.top.GetHashCode() + this.right.GetHashCode()
                      + this.bottom.GetHashCode());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" />
        /// parameter; otherwise, <see langword="false" />.
        /// </returns>
        /// <seealso cref="M:System.IEquatable{OpenSky.Client.Native.PInvoke.Structs.Rect}.Equals(Rect)"/>
        /// -------------------------------------------------------------------------------------------------
        public bool Equals(Rect other)
        {
            return this == other;
        }
    }
}