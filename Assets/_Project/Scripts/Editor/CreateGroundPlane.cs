using UnityEngine;
using UnityEditor;

public class CreateGroundPlane {
    [MenuItem("Tools/Create Ground Plane")]
    public static void CreateGround() {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Ground";
        plane.transform.position = Vector3.zero;
        plane.transform.localScale = new Vector3(100f, 1f, 100f);
        
        System.IO.Directory.CreateDirectory("Assets/_Project/Materials");
        
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.2f, 0.2f, 0.2f);
        
        // We might not have URP installed, so fallback to Standard
        if (mat.shader == null) {
            mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.2f, 0.2f);
        }
        
        AssetDatabase.CreateAsset(mat, "Assets/_Project/Materials/GroundMaterial.mat");
        
        plane.GetComponent<MeshRenderer>().sharedMaterial = mat;
        
        plane.layer = LayerMask.NameToLayer("Default");
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Ground Plane created.");
    }
}
