﻿// Copyright (c) Lex Li. All rights reserved.
// 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

//
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

//#if NET_2_0

using System;
using System.Collections;

namespace Microsoft.Web.Administration
{
    [Flags]
    internal enum ConfigurationLockType
    {
        Attribute = 0x01,
        Element = 0x02,
        Exclude = 0x10
    }

    public sealed class ConfigurationLockCollection : ICollection
    {
        private ArrayList _names;
        private ConfigurationElement _element;
        private ConfigurationLockType _lockType;
        private bool _is_modified;
        private Hashtable _valid_name_hash;
        private string _valid_names;

        internal ConfigurationLockCollection(ConfigurationElement element,
                              ConfigurationLockType lockType)
        {
            _names = new ArrayList();
            _element = element;
            _lockType = lockType;
        }

        private void CheckName(string name)
        {
            bool isAttribute = (_lockType & ConfigurationLockType.Attribute) == ConfigurationLockType.Attribute;

            if (_valid_name_hash == null)
            {
                _valid_name_hash = new Hashtable();
                foreach (ConfigurationAttributeSchema prop in _element.Schema.AttributeSchemas)
                {
                    _valid_name_hash.Add(prop.Name, true);
                }

                /* add the add/remove/clear names of the
                 * default collection if there is one */
                if (!isAttribute)
                {
                    ConfigurationElementCollection c = _element.GetCollection();
                    foreach (var addName in _element.Schema.CollectionSchema.AddElementNames.Split(','))
                    {
                        _valid_name_hash.Add(addName, true);
                    }

                    _valid_name_hash.Add(c.Schema.CollectionSchema.ClearElementName, true);
                    _valid_name_hash.Add(c.Schema.CollectionSchema.RemoveElementName, true);
                }

                string[] valid_name_array = new string[_valid_name_hash.Keys.Count];
                _valid_name_hash.Keys.CopyTo(valid_name_array, 0);

                _valid_names = String.Join(",", valid_name_array);
            }

            if (_valid_name_hash[name] == null)
                throw new ServerManagerException(
                        String.Format("The {2} '{0}' is not valid in the locked list for this section.  The following {3} can be locked: '{1}'",
                                   name, _valid_names, isAttribute ? "attribute" : "element", isAttribute ? "attributes" : "elements"));
        }

        public void Add(string name)
        {
            CheckName(name);
            if (!_names.Contains(name))
            {
                _names.Add(name);
                _is_modified = true;
            }
        }

        public void Clear()
        {
            _names.Clear();
            _is_modified = true;
        }

        public bool Contains(string name)
        {
            return _names.Contains(name);
        }

        public void CopyTo(string[] array, int index)
        {
            _names.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _names.GetEnumerator();
        }

        //[MonoInternalNote ("we can't possibly *always* return false here...")]
        public bool IsReadOnly(string name)
        {
            for (int i = 0; i < _names.Count; i++)
            {
                if ((string)_names[i] == name)
                {
                    /* this test used to switch off whether the collection was 'Exclude' or not
                     * (the LockAll*Except collections), but that doesn't seem to be the crux of
                     * it.  maybe this returns true if the element/attribute is locked in a parent
                     * element's lock collections? */
                    return false;
                }
            }

            throw new ServerManagerException(String.Format("The entry '{0}' is not in the collection.", name));
        }

        public void Remove(string name)
        {
            _names.Remove(name);
            _is_modified = true;
        }

        public void SetFromList(string attributeList)
        {
            Clear();

            char[] split = { ',' };
            string[] attrs = attributeList.Split(split, StringSplitOptions.RemoveEmptyEntries);
            foreach (string a in attrs)
            {
                Add(a.Trim());
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _names.CopyTo(array, index);
        }

        public string AttributeList
        {
            get
            {
                string[] name_arr = new string[_names.Count];
                _names.CopyTo(name_arr, 0);
                return string.Join(",", name_arr);
            }
        }

        public int Count
        {
            get { return _names.Count; }
        }

        //[MonoTODO]
        public bool HasParentElements
        {
            get { return false; /* XXX */ }
        }

        //[MonoTODO]
        public bool IsModified
        {
            get { return _is_modified; }
            internal set { _is_modified = value; }
        }

        //[MonoTODO]
        public bool IsSynchronized
        {
            get { return false; /* XXX */ }
        }

        //[MonoTODO]
        public object SyncRoot
        {
            get { return this; /* XXX */ }
        }
    }
}

//#endif
