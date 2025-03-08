using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class CutoutSync : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_PlayerPosition");
    public static int SizeID = Shader.PropertyToID("_Size");
    
    public Material material;
    public Camera camera;
    public LayerMask layerMask;
    void Update()
    {
        var dir = camera.transform.position - transform.position;
        var ray = new Ray(transform.position, dir.normalized);
        
        if (Physics.Raycast(ray, 3000, layerMask))
            material.SetFloat(SizeID, 1);
        else
            material.SetFloat(SizeID, 0);
        
        var view = camera.WorldToViewportPoint(transform.position);
        material.SetVector(PosID, view);
    }
    
    /*
     * Renderer rend = GetComponent<Renderer>();

       bool hit = Physics.Raycast(ray, 3000, layerMask);
       
       if (rend != null)
       {
           foreach (var mat in rend.materials)
           {
               mat.SetFloat(SizeID, hit ? 1 : 0);
               
               var view = camera.WorldToViewportPoint(transform.position);
               mat.SetVector(PosID, view);
           }
       }
     */
}
