//-----------------------------------------------------------------------
// <copyright file="CSharpTypeResolver.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;

namespace NJsonSchema.CodeGeneration.TypeScript
{
    /// <summary>Manages the generated types and converts JSON types to CSharp types. </summary>
    public class TypeScriptTypeResolver : TypeResolverBase<TypeScriptGenerator>
    {
        /// <summary>Initializes a new instance of the <see cref="TypeScriptTypeResolver"/> class.</summary>
        public TypeScriptTypeResolver()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TypeScriptTypeResolver"/> class.</summary>
        /// <param name="knownSchemes">The known schemes.</param>
        public TypeScriptTypeResolver(JsonSchema4[] knownSchemes)
        {
            foreach (var type in knownSchemes)
                AddOrReplaceTypeGenerator(type.TypeName, new TypeScriptGenerator(type.ActualSchema, this));
        }

        /// <summary>Gets or sets the namespace of the generated classes.</summary>
        public string Namespace { get; set; }

        /// <summary>Resolves and possibly generates the specified schema.</summary>
        /// <param name="schema">The schema.</param>
        /// <param name="isNullable">Specifies whether the given type usage is nullable.</param>
        /// <param name="typeNameHint">The type name hint to use when generating the type and the type name is missing.</param>
        /// <returns>The type name.</returns>
        public override string Resolve(JsonSchema4 schema, bool isNullable, string typeNameHint)
        {
            schema = schema.ActualSchema; 

            var type = schema.Type;
            if (type.HasFlag(JsonObjectType.Array))
            {
                var property = schema;
                if (property.Item != null)
                    return string.Format("{0}[]", Resolve(property.Item, true, null));

                throw new NotImplementedException("Array with multiple Items schemes are not supported.");
            }

            if (type.HasFlag(JsonObjectType.Number))
                return "number";

            if (type.HasFlag(JsonObjectType.Integer))
            {
                if (schema.IsEnumeration)
                    return AddGenerator(schema, typeNameHint);

                return "number";
            }

            if (type.HasFlag(JsonObjectType.Boolean))
                return "boolean";

            if (type.HasFlag(JsonObjectType.String))
            {
                if (schema.Format == JsonFormatStrings.DateTime)
                    return "Date";

                if (schema.IsEnumeration)
                    return AddGenerator(schema, typeNameHint);
                
                return "string";
            }

            if (schema.IsAnyType)
                return "any";

            if (schema.IsDictionary)
                return string.Format("{{ [key: string] : {0}; }}", Resolve(schema.AdditionalPropertiesSchema, true, null));

            return AddGenerator(schema, typeNameHint);
        }

        /// <summary>Creates a type generator.</summary>
        /// <param name="schema">The schema.</param>
        /// <returns>The generator.</returns>
        protected override TypeScriptGenerator CreateTypeGenerator(JsonSchema4 schema)
        {
            return new TypeScriptGenerator(schema, this);
        }

        /// <summary>Gets or generates the type name for the given schema.</summary>
        /// <param name="schema">The schema.</param>
        /// <param name="typeNameHint">The type name hint.</param>
        /// <returns>The type name.</returns>
        protected override string GetOrGenerateTypeName(JsonSchema4 schema, string typeNameHint)
        {
            var typeName = base.GetOrGenerateTypeName(schema, typeNameHint);

            if (schema.IsEnumeration && schema.Type == JsonObjectType.Integer)
                return typeName + "AsInteger";

            return typeName;
        }
    }
}