﻿// Copyright (c) 2013 SharpYaml - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// -------------------------------------------------------------------------------
// SharpYaml is a fork of YamlDotNet https://github.com/aaubry/YamlDotNet
// published with the following license:
// -------------------------------------------------------------------------------
// 
// Copyright (c) 2008, 2009, 2010, 2011, 2012 Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.Reflection;
using SharpYaml.Schemas;
using SharpYaml.Serialization.Serializers;

namespace SharpYaml.Serialization
{
	/// <summary>
	/// Settings used to configure serialization and control how objects are encoded into YAML.
	/// </summary>
	public sealed class SerializerSettings
	{
		internal readonly List<IYamlSerializableFactory> factories = new List<IYamlSerializableFactory>();
		internal readonly Dictionary<Type, IYamlSerializable> serializers = new Dictionary<Type, IYamlSerializable>();
		internal readonly TagTypeRegistry tagTypeRegistry;
		private IAttributeRegistry attributeRegistry;
		private readonly IYamlSchema schema;
		private IObjectFactory objectFactory;
		private int preferredIndent;
		private string specialCollectionMember;
	    private IObjectSerializerBackend objectSerializerBackend;

	    /// <summary>
		/// Initializes a new instance of the <see cref="SerializerSettings"/> class.
		/// </summary>
		public SerializerSettings() : this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializerSettings" /> class.
		/// </summary>
		public SerializerSettings(IYamlSchema schema)
		{
			PreferredIndent = 2;
			IndentLess = false;
			EmitAlias = true;
		    EmitTags = true;
			SortKeyForMapping = true;
			EmitJsonComptible = false;
			EmitCapacityForList = false;
			SpecialCollectionMember = "~Items";
			LimitPrimitiveFlowSequence = 0;
			DefaultStyle = YamlStyle.Block;
			this.schema = schema ?? new CoreSchema();
			tagTypeRegistry = new TagTypeRegistry(Schema);
			attributeRegistry = new AttributeRegistry();
			ObjectFactory = new DefaultObjectFactory();
            ObjectSerializerBackend = new DefaultObjectSerializerBackend();

			// Register default mapping for map and seq
			tagTypeRegistry.RegisterTagMapping("!!map", typeof(IDictionary<object, object>));
			tagTypeRegistry.RegisterTagMapping("!!seq", typeof(IList<object>));
		}

		/// <summary>
		/// Gets or sets the preferred indentation. Default is 2.
		/// </summary>
		/// <value>The preferred indentation.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">value;Expecting value &gt; 0</exception>
		public int PreferredIndent
		{
			get { return preferredIndent; }
			set
			{
				if (value < 1) throw new ArgumentOutOfRangeException("value", "Expecting value > 0");
				preferredIndent = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to emit anchor alias. Default is true.
		/// </summary>
		/// <value><c>true</c> to emit anchor alias; otherwise, <c>false</c>.</value>
		public bool EmitAlias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to emit tags when serializing. Default is true.
        /// </summary>
        /// <value><c>true</c> to emit tags when serializing; otherwise, <c>false</c>.</value>
        public bool EmitTags { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the identation is trying to less
		/// indent when possible
		/// (For example, sequence after a key are not indented). Default is false.
		/// </summary>
		/// <value><c>true</c> if [always indent]; otherwise, <c>false</c>.</value>
		public bool IndentLess { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to enable sorting keys from dictionary to YAML mapping. Default is true. See remarks.
		/// </summary>
		/// <value><c>true</c> to enable sorting keys from dictionary to YAML mapping; otherwise, <c>false</c>.</value>
		/// <remarks>When storing a YAML document, It can be important to keep the same order for key mapping in order to keep
		/// a YAML document versionable/diffable.</remarks>
		public bool SortKeyForMapping { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to emit JSON compatible YAML.
		/// </summary>
		/// <value><c>true</c> if to emit JSON compatible YAML; otherwise, <c>false</c>.</value>
		public bool EmitJsonComptible { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the property <see cref="List{T}.Capacity" /> should be emitted. Default is false.
		/// </summary>
		/// <value><c>true</c> if the property <see cref="List{T}.Capacity" /> should be emitted; otherwise, <c>false</c>.</value>
		public bool EmitCapacityForList { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of elements an array/list of primitive can be emitted as a
		/// flow sequence (instead of a block sequence by default). Default is 0, meaning block style
		/// for all sequuences.
		/// </summary>
		/// <value>The emit compact array limit.</value>
		public int LimitPrimitiveFlowSequence { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to emit default value. Default is false.
		/// </summary>
		/// <value><c>true</c> if to emit default value; otherwise, <c>false</c>.</value>
		public bool EmitDefaultValues { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to emit short type name (type, assembly name) or full <see cref="Type.AssemblyQualifiedName"/>. Default is false.
		/// </summary>
		/// <value><c>true</c> to emit short type name; otherwise, <c>false</c>.</value>
		public bool EmitShortTypeName
		{
			get { return tagTypeRegistry.UseShortTypeName; }
			set { tagTypeRegistry.UseShortTypeName = value; }
		}

		/// <summary>
		/// Gets or sets the default <see cref="YamlStyle"/>. Default is <see cref="YamlStyle.Block"/>. See <see cref="DynamicStyleFormat"/> to understand the resolution of styles.
		/// </summary>
		/// <value>The default style.</value>
		public YamlStyle DefaultStyle { get; set; }

		/// <summary>
		/// Gets or sets the prefix used to serialize items for a non pure <see cref="System.Collections.IDictionary" /> or
		/// <see cref="System.Collections.ICollection" />
		/// . Default to "~Items", see remarks.
		/// </summary>
		/// <value>The prefix for items.</value>
		/// <exception cref="System.ArgumentNullException">value</exception>
		/// <exception cref="System.ArgumentException">Expecting length >= 2 and at least a special character '.', '~', '-' (not starting on first char for '-')</exception>
		/// <remarks>A pure <see cref="System.Collections.IDictionary" /> or <see cref="System.Collections.ICollection" /> is a class that inherits from these types but are not adding any
		/// public properties or fields. When these types are pure, they are respectively serialized as a YAML mapping (for dictionary) or a YAML sequence (for collections).
		/// If the collection type to serialize is not pure, the type is serialized as a YAML mapping sequence that contains the public properties/fields as well as a
		/// special fielx (e.g. "~Items") that contains the actual items of the collection (either a mapping for dictionary or a sequence for collections).
		/// The <see cref="SpecialCollectionMember" /> is this special key that is used when serializing items of a non-pure collection.</remarks>
		public string SpecialCollectionMember
		{
			get { return specialCollectionMember; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");

				// TODO this is a poor check. Need to verify this against the specs
				if (value.Length < 2 || !(value.Contains(".") || value.Contains("~") || value.IndexOf('-') > 0))
				{
					throw new ArgumentException(
						"Expecting length >= 2 and at least a special character '.', '~', '-' (not starting on first char for '-')");
				}

				specialCollectionMember = value;
			}
		}

		/// <summary>
		/// Gets the attribute registry.
		/// </summary>
		/// <value>The attribute registry.</value>
		public IAttributeRegistry Attributes
		{
			get { return attributeRegistry; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				attributeRegistry = value;
			}
		}

	    /// <summary>
	    /// Gets or sets the ObjectSerializerBackend. Default implementation is <see cref="DefaultObjectSerializerBackend"/>
	    /// </summary>
	    /// <value>The ObjectSerializerBackend.</value>
	    public IObjectSerializerBackend ObjectSerializerBackend
	    {
	        get { return objectSerializerBackend; }
	        set
	        {
                if (value == null) throw new ArgumentNullException("value");
                objectSerializerBackend = value;
	        }
	    }

	    /// <summary>
	    /// Gets or sets the default factory to instantiate a type. Default is <see cref="DefaultObjectFactory" />.
	    /// </summary>
	    /// <value>The default factory to instantiate a type.</value>
	    /// <exception cref="System.ArgumentNullException">value</exception>
	    public IObjectFactory ObjectFactory
		{
			get { return objectFactory; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				objectFactory = value;
			}
		}

		/// <summary>
		/// Gets or sets the schema. Default is <see cref="CoreSchema" />.
		/// method.
		/// </summary>
		/// <value>The schema.</value>
		/// <exception cref="System.ArgumentNullException">value</exception>
		public IYamlSchema Schema
		{
			get { return schema; }
		}

		/// <summary>
		/// Register a mapping between a tag and a type.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		public void RegisterAssembly(Assembly assembly)
		{
			tagTypeRegistry.RegisterAssembly(assembly, attributeRegistry);
		}

		/// <summary>
		/// Register a mapping between a tag and a type.
		/// </summary>
		/// <param name="tagName">Name of the tag.</param>
		/// <param name="tagType">Type of the tag.</param>
		public void RegisterTagMapping(string tagName, Type tagType)
		{
			tagTypeRegistry.RegisterTagMapping(tagName, tagType);
		}

		/// <summary>
		/// Adds a custom serializer for the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="serializer">The serializer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// type
		/// or
		/// serializer
		/// </exception>
		public void RegisterSerializer(Type type, IYamlSerializable serializer)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (serializer == null) throw new ArgumentNullException("serializer");
			serializers[type] = serializer;
		}

		/// <summary>
		/// Adds a serializer factory.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <exception cref="System.ArgumentNullException">factory</exception>
		public void RegisterSerializerFactory(IYamlSerializableFactory factory)
		{
			if (factory == null) throw new ArgumentNullException("factory");
			factories.Add(factory);
		}
	}
}