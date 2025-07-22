using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public enum AlignmentMode
{
    Left,
    Center
}

[AddComponentMenu("UI/Effects/TextSpacing")]
public class TextSpacing : BaseMeshEffect
{
    [SerializeField]
    public float spacing_x;
    [SerializeField]
    public float spacing_y;

    [SerializeField]
    public AlignmentMode alignment = AlignmentMode.Center;

    private List<UIVertex> mVertexList;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (spacing_x == 0 && spacing_y == 0) { return; }
        if (!IsActive()) { return; }
        int count = vh.currentVertCount;
        if (count == 0) { return; }
        if (mVertexList == null) { mVertexList = new List<UIVertex>(); }
        vh.GetUIVertexStream(mVertexList);

        int vertex_count = mVertexList.Count;
        List<List<UIVertex>> lines = new List<List<UIVertex>>();
        List<UIVertex> currentLine = new List<UIVertex>();

        float last_left = mVertexList[0].position.x;
        for (int i = 0; i < vertex_count; i += 6)
        {
            var sub = mVertexList.GetRange(i, 6);
            float left = sub.Min(v => v.position.x);
            if (lines.Count == 0 || Mathf.Abs(left - last_left) < 1e-3)
            {
                currentLine.AddRange(sub);
            }
            else
            {
                lines.Add(currentLine);
                currentLine = new List<UIVertex>();
                currentLine.AddRange(sub);
            }
            last_left = left;
        }
        if (currentLine.Count > 0)
            lines.Add(currentLine);

        int charIndex = 0;
        for (int row = 0; row < lines.Count; row++)
        {
            var line = lines[row];
            int charCount = line.Count / 6;
            float lineWidth = (charCount - 1) * spacing_x;
            float centerOffset = alignment == AlignmentMode.Center ? -lineWidth / 2f : 0f;

            for (int col = 0; col < charCount; col++)
            {
                for (int v = 0; v < 6; v++)
                {
                    int idx = charIndex + col * 6 + v;
                    UIVertex vertex = mVertexList[idx];
                    vertex.position += Vector3.right * (col * spacing_x + centerOffset);
                    vertex.position += Vector3.down * (row * spacing_y);
                    mVertexList[idx] = vertex;
                }
            }
            charIndex += line.Count;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(mVertexList);
    }
}