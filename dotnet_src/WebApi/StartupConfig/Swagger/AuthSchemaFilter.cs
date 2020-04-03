﻿using System;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.StartupConfig.Swagger
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            schema.Example = GetExampleOrNullFor(context.Type);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IOpenApiAny GetExampleOrNullFor(Type type)
        {
            switch (type.Name)
            {
                case "Product":
                    return new OpenApiObject
                    {
                        ["id"] = new OpenApiInteger(123),
                        ["description"] = new OpenApiString("foobar"),
                        ["price"] = new OpenApiDouble(14.37)
                    };
                default:
                    return null;
            }
        }
    }
}