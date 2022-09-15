// <copyright file="ZipkinTagTransformer.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//

namespace Dora.OpenTelemetry.Zipkin
{

    internal sealed class ZipkinTagTransformer : TagTransformer<string>
    {
        private ZipkinTagTransformer() { }
        public static ZipkinTagTransformer Instance { get; } = new();
        protected override string TransformInt64(string key, long value) => value.ToString();
        protected override string TransformDouble(string key, double value) => value.ToString();
        protected override string TransformBoolean(string key, bool value) => value ? "true" : "false";
        protected override string TransformString(string key, string value) => value;
        protected override string TransformArray(string key, Array array) => TransformString(key, System.Text.Json.JsonSerializer.Serialize(array));
    }
}
