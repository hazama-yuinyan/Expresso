using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.NRefactory;
using LanguageServer.VsCode.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ExpressoLanguageServer
{
    internal static class Utility
    {
        public static readonly JsonSerializer CamelCaseJsonSerializer = new JsonSerializer{
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Gets a <see cref="TextLocation"/> from <see cref="Position"/>.
        /// </summary>
        /// <returns>The text location from position.</returns>
        /// <param name="position">Position.</param>
        public static TextLocation GetTextLocationFromPosition(Position position)
        {
            return new TextLocation(position.Line + 1, position.Character + 1);
        }

        /// <summary>
        /// Gets a <see cref="Position"/> from <see cref="TextLocation"/>.
        /// </summary>
        /// <returns>The position from text location.</returns>
        /// <param name="location">Location.</param>
        public static Position GetPositionFromTextLocation(TextLocation location)
        {
            return new Position(location.Line - 1, location.Column - 1);
        }
    }
}
