using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.AI;

namespace MyOllamaHub3
{
    internal static class StreamingUpdateHelper
    {
        private static readonly MethodInfo? GetContentStringMethod = typeof(ChatResponseUpdate).GetMethod("GetContentString", Type.EmptyTypes);
        private static readonly PropertyInfo? ContentUpdateProperty = typeof(ChatResponseUpdate).GetProperty("ContentUpdate");
        private static readonly PropertyInfo? ResponseMessageProperty = typeof(ChatResponseUpdate).GetProperty("ResponseMessage");

        public static string ExtractText(ChatResponseUpdate? update)
        {
            if (update == null) return string.Empty;

            var direct = update.Text;
            if (!string.IsNullOrEmpty(direct))
                return direct;

            if (GetContentStringMethod != null)
            {
                if (GetContentStringMethod.Invoke(update, Array.Empty<object>()) is string viaMethod && !string.IsNullOrEmpty(viaMethod))
                    return viaMethod;
            }

            var fromUpdate = ExtractFromParts(ContentUpdateProperty?.GetValue(update));
            if (!string.IsNullOrEmpty(fromUpdate))
                return fromUpdate;

            var response = ResponseMessageProperty?.GetValue(update);
            if (response != null)
            {
                var contentProperty = response.GetType().GetProperty("Content");
                if (contentProperty != null)
                {
                    var fromResponse = ExtractFromParts(contentProperty.GetValue(response));
                    if (!string.IsNullOrEmpty(fromResponse))
                        return fromResponse;
                }
            }

            return string.Empty;
        }

        private static string ExtractFromParts(object? parts)
        {
            if (parts is not IEnumerable enumerable) return string.Empty;

            var sb = new StringBuilder();
            foreach (var item in enumerable)
            {
                if (item == null) continue;
                var textProperty = item.GetType().GetProperty("Text");
                if (textProperty == null) continue;

                if (textProperty.GetValue(item) is string text && !string.IsNullOrEmpty(text))
                    sb.Append(text);
            }

            return sb.ToString();
        }

        public static string StripHiddenSections(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var input = text!;
            var builder = new StringBuilder(input.Length);
            var index = 0;
            var suppressing = false;

            while (index < input.Length)
            {
                if (suppressing)
                {
                    var close = input.IndexOf("</think>", index, StringComparison.OrdinalIgnoreCase);
                    if (close < 0)
                        break;

                    index = close + "</think>".Length;
                    suppressing = false;
                    continue;
                }

                var start = input.IndexOf("<think", index, StringComparison.OrdinalIgnoreCase);
                if (start < 0)
                {
                    builder.Append(input, index, input.Length - index);
                    break;
                }

                if (start > index)
                    builder.Append(input, index, start - index);

                var endOfTag = input.IndexOf('>', start);
                if (endOfTag < 0)
                {
                    suppressing = true;
                    break;
                }

                index = endOfTag + 1;
                suppressing = true;
            }

            var sanitized = builder.ToString();
            if (sanitized.IndexOf("</think>", StringComparison.OrdinalIgnoreCase) >= 0)
                sanitized = sanitized.Replace("</think>", string.Empty, StringComparison.OrdinalIgnoreCase);

            return sanitized;
        }
    }
}
