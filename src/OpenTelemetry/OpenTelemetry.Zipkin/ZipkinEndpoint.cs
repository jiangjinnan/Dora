// <copyright file="ZipkinEndpoint.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;

namespace Dora.OpenTelemetry.Zipkin
{
    public class ZipkinEndpoint
    {
        public string ServiceName { get; }
        public string? Ipv4 { get; }
        public string? Ipv6 { get; }
        public int? Port { get; }
        public Dictionary<string, object> Tags { get; } = new();
        public ZipkinEndpoint(string serviceName, string? ipv4, string? ipv6, int? port)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            Ipv4 = ipv4;
            Ipv6 = ipv6;
            Port = port;
        }
    }
}
