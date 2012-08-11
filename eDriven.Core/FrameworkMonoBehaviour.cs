﻿/*
 
Copyright (c) 2012 Danko Kozar

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 
*/


using System.Reflection;
using UnityEngine;

namespace eDriven.Core
{
    /// <summary>
    /// The hack
    /// The only purpose for now for having this Script is to make FrameworkObject not destroyable, 
    /// so it can walk from scene to scene
    /// This is done by calling DontDestroyOnLoad() in Awake() method
    /// </summary>
    public sealed class FrameworkMonoBehaviour : MonoBehaviour
    {
        private bool _initialized;

        // ReSharper disable UnusedMember.Local
        [Obfuscation(Exclude = true)]
        void Awake()
        // ReSharper restore UnusedMember.Local
        {
            if (!_initialized)
            {
                _initialized = true;
                //Debug.Log("FrameworkMonoBehaviour.Awake");

                /**
                 * If you’d like your singleton to remain between scene loads just add:
                 * “DontDestroyOnLoad(container);” just after you create it. 
                 * This prevents Unity from destroying it and means you can use it to stored data between scenes.
                 * */
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}