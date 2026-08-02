using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PhotoData
{
    public Texture2D tex;
    public List<EvidenceData> datas;
}

public static class PhotoDataHelper
{
    public static PhotoData CreatePhotoData(Texture2D croppedTexture, List<EvidenceData> evidences)
    {
        return new PhotoData
        {
            tex = CopyTexture(croppedTexture),
            datas = CopyEvidenceDatas(evidences)
        };
    }
    
    private static Texture2D CopyTexture(Texture2D source)
    {
        if (source == null)
            return null;

        Texture2D copy = new Texture2D(
            source.width,
            source.height,
            source.format,
            source.mipmapCount > 1
        );

        copy.SetPixels(source.GetPixels());
        copy.Apply();

        copy.filterMode = source.filterMode;
        copy.wrapMode = source.wrapMode;
        copy.anisoLevel = source.anisoLevel;
        copy.name = $"{source.name}_Copy";

        return copy;
    }

    private static List<EvidenceData> CopyEvidenceDatas(
        List<EvidenceData> source)
    {
        if (source == null)
            return new List<EvidenceData>();

        List<EvidenceData> result =
            new List<EvidenceData>(source.Count);

        foreach (var evidence in source)
        {
            result.Add(evidence);
        }

        return result;
    }
}