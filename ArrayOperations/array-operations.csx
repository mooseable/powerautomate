// --- C# Script: array-operations.csx (for sumBySchema) ---

using System.Net;
using Newtonsoft.Json.Linq;

public class Script : ScriptBase
{
    public override async Task<HttpResponseMessage> ExecuteAsync()
    {
        var body = await Context.Request.Content.ReadAsStringAsync();
        JObject json;

        try { json = JObject.Parse(body); }
        catch { return CreateBadRequest("Invalid JSON body."); }

        try
        {
            switch (Context.OperationId)
            {
                case "sumBySchema":
                    return HandleSumBySchema(json);
                default:
                    return CreateBadRequest($"Unsupported operation: {Context.OperationId}");
            }
        }
        catch (Exception ex)
        {
            return CreateBadRequest($"Unhandled error: {ex.Message}");
        }
    }

    private HttpResponseMessage HandleSumBySchema(JObject json)
    {
        var schema = json["schema"]?.ToObject<JObject>();
        if (schema == null)
            return CreateBadRequest("Missing or invalid 'schema'.");

        if (json["array"] is JArray array)
        {
            return HandleSchemaFromArray(array, schema);
        }
        else if (json["object"] is JObject obj)
        {
            return HandleSchemaFromObject(obj, schema);
        }

        return CreateBadRequest("Missing or invalid 'array' or 'object' input.");
    }

    private HttpResponseMessage HandleSchemaFromArray(JArray array, JObject schema)
    {
        var result = new JObject();
        var totals = new Dictionary<string, double>();

        foreach (var section in schema)
        {
            string parent = section.Key;
            var props = section.Value as JArray;
            if (props == null) continue;

            foreach (var propToken in props)
            {
                string propName = propToken.ToString();
                double sum = 0;

                foreach (var item in array)
                {
                    JToken value = null;

                    if (string.IsNullOrEmpty(parent))
                    {
                        value = item.Type == JTokenType.Object ? item[propName] : item;
                    }
                    else
                    {
                        var parentToken = item[parent];
                        if (parentToken != null && parentToken.Type == JTokenType.Object)
                        {
                            value = parentToken[propName];
                        }
                    }

                    if (value != null && double.TryParse(value.ToString(), out double val))
                    {
                        sum += val;
                        if (!totals.ContainsKey(propName)) totals[propName] = 0;
                        totals[propName] += val;
                    }
                }

                result[$"{(string.IsNullOrEmpty(parent) ? "" : parent + ".")}{propName}.sum"] = sum;
            }
        }

        foreach (var kvp in totals)
        {
            result[$"{kvp.Key}.sum"] = kvp.Value;
        }

        return CreateJsonResponse(result);
    }

    private HttpResponseMessage HandleSchemaFromObject(JObject obj, JObject schema)
    {
        var result = new JObject();
        var totals = new Dictionary<string, double>();

        foreach (var section in schema)
        {
            string sectionKey = section.Key;
            var props = section.Value as JArray;

            if (props == null || obj[sectionKey] == null || !(obj[sectionKey] is JArray))
                continue;

            var sectionArray = obj[sectionKey] as JArray;

            foreach (var propToken in props)
            {
                string propName = propToken.ToString();
                double sum = 0;

                foreach (var item in sectionArray)
                {
                    var value = item[propName];
                    if (value != null && double.TryParse(value.ToString(), out double val))
                    {
                        sum += val;
                        if (!totals.ContainsKey(propName)) totals[propName] = 0;
                        totals[propName] += val;
                    }
                }

                result[$"{sectionKey}.{propName}.sum"] = sum;
            }
        }

        foreach (var kvp in totals)
        {
            result[$"{kvp.Key}.sum"] = kvp.Value;
        }

        return CreateJsonResponse(result);
    }

    private HttpResponseMessage CreateBadRequest(string message)
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = CreateJsonContent(new JObject { ["error"] = message }.ToString())
        };
    }

    private HttpResponseMessage CreateJsonResponse(JObject json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = CreateJsonContent(json.ToString())
        };
    }
} // end Script class
