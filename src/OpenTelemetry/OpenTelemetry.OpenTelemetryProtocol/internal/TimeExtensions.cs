// <copyright file="TimestampHelpers.cs" company="OpenTelemetry Authors">
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

namespace Dora.OpenTelemetry.OpenTelemetryProtocol
{

    internal static class TimeExtensions
    {
        private const long NanosecondsPerTicks = 100;
        private const long UnixEpochTicks = 621355968000000000; 

        internal static long ToUnixTimeNanoseconds(this DateTime dt)=> (dt.Ticks - UnixEpochTicks) * NanosecondsPerTicks;
        internal static long ToUnixTimeNanoseconds(this DateTimeOffset dto)=> (dto.Ticks - UnixEpochTicks) * NanosecondsPerTicks;
        internal static long ToNanoseconds(this TimeSpan duration)=> duration.Ticks * NanosecondsPerTicks;
    }
}
