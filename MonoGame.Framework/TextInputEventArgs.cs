// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// This class is used in the <see cref="GameWindow.TextInput"/> event as <see cref="EventArgs"/>.
    /// </summary>
    public class TextInputEventArgs : EventArgs
    {
        char character;

        /// <summary>
        /// Create a <see cref="TextInputEventArgs"/> instance.
        /// </summary>
        /// <param name="character">The character for the key that was pressed.</param>
        /// <param name="key">The pressed key.</param>
        public TextInputEventArgs(char character, Keys key = Keys.None)
        {
            this.character = character;
            this.Key = key;
        }

        /// <summary>
        /// The character for the key that was pressed.
        /// </summary>
        public char Character
        {
            get
            {
                return character;
            }
        }

        /// <summary>
        /// The pressed key.
        /// </summary>
        public Keys Key {
            get; private set;
        }
    }
}
