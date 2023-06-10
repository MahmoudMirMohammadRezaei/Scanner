﻿using NAPS2.Serialization;

namespace NAPS2.Scan;

/// <summary>
/// A set of key-value options used for scanning.
///
/// This is only relevant for SANE. Currently NAPS2 does not actually support viewing/setting custom options.
/// If someone was so inclined they could manually set them in the profiles.xml file.
/// </summary>
public class KeyValueScanOptions : Dictionary<string, string>
{
    static KeyValueScanOptions()
    {
        XmlSerializer.RegisterCustomSerializer(new Serializer());
    }

    public KeyValueScanOptions()
    {
    }

    public KeyValueScanOptions(IDictionary<string, string> dictionary) : base(dictionary)
    {
    }

    private class Serializer : CustomXmlSerializer<KeyValueScanOptions>
    {
        protected override void Serialize(KeyValueScanOptions obj, XElement element)
        {
            foreach (var kvp in obj)
            {
                var itemElement = new XElement("Option", kvp.Value);
                itemElement.SetAttributeValue("name", kvp.Key);
                element.Add(itemElement);
            }
        }

        protected override KeyValueScanOptions Deserialize(XElement element)
        {
            var obj = new KeyValueScanOptions();
            foreach (var itemElement in element.Elements())
            {
                var name = itemElement.Attribute("name")?.Value ??
                           throw new InvalidOperationException("Could not read option name");
                obj.Add(name, itemElement.Value);
            }
            return obj;
        }
    }
}