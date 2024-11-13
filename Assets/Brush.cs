using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

//Need to add
    //Collision
    //3D points

public class Brush : MonoBehaviour
{
    public GameObject spot, wall;
    public bool canChangeSize;
    public float cooldown;
    
    Vector3 previous;
    Vector3[] previousPerpPoints;
    float distance;
    bool wallStart;

    // Start is called before the first frame update
    void Start()
    {   
        spot.transform.localScale = new Vector3(1f, 0.001f, 1f);
        canChangeSize = true;
        cooldown = 0.1f;
        distance = 2f;
        wallStart = false;
        previousPerpPoints = new Vector3[] {Vector3.up, Vector3.up};
    }

    // Update is called once per frame
    void Update()
    {
        if(canChangeSize){
            if(Input.GetButton("IncreaseSize")){
                if(spot.transform.localScale.x < 5){
                    StartCoroutine(changeSize(0.5f));
                }
            }
            if(Input.GetButton("DecreaseSize")){
                if(spot.transform.localScale.x > 0.5){
                    StartCoroutine(changeSize(-0.5f));
                }
            }
        }
       
    }

    void FixedUpdate(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawLine(ray.origin, Camera.main.transform.forward * 500, Color.red);

        int layerMask = 1 << 3;

        RaycastHit hit; 

        bool success = Physics.Raycast(ray, out hit, 100f, layerMask);
        if(success){
            spot.transform.position = hit.point;
        }
         if(Input.GetMouseButton(0)){
            if(!wallStart){
                previous = hit.point;
                wallStart = true;
            }
            else if(distanceFrom(previous, hit.point) >= distance){
                Vector3[] pointArr = points(previous, hit.point, Vector3.up);
                //right mesh
                Mesh m = createMesh(new Vector3[]{pointArr[0], pointArr[1], pointArr[0]+Vector3.up, pointArr[1]+Vector3.up}, pointArr[4], new int[] {0, 2, 1, 1, 2, 3});
                //left mesh
                m = createMesh(new Vector3[]{pointArr[2], pointArr[3], pointArr[2]+Vector3.up, pointArr[3]+Vector3.up}, -pointArr[4], new int[] {0, 1, 2, 2, 1, 3});
                //top mesh
                m = createMesh(new Vector3[]{pointArr[0]+Vector3.up, pointArr[1]+Vector3.up, pointArr[2]+Vector3.up, pointArr[3]+Vector3.up}, Vector3.up, new int[] {0, 2, 1, 1, 2, 3});

                //previous meshes
                if(previousPerpPoints[0] != Vector3.up){
                    //prev right mesh
                    m = createMesh(new Vector3[] {previousPerpPoints[1], pointArr[3], previousPerpPoints[1]+Vector3.up, pointArr[3]+Vector3.up}, -pointArr[4], new int[] {0, 2, 1, 1, 2, 3});
                    //prev left mesh
                    m = createMesh(new Vector3[] {previousPerpPoints[0], pointArr[1], previousPerpPoints[0]+Vector3.up, pointArr[1]+Vector3.up}, pointArr[4], new int[] {0, 1, 2, 2, 1, 3});
                    //prev top mesh
                    m = createMesh(new Vector3[] {previousPerpPoints[0]+Vector3.up, pointArr[1]+Vector3.up, previousPerpPoints[1]+Vector3.up, pointArr[3]+Vector3.up}, Vector3.up, new int[] {0, 1, 2, 0,2,3});
                m.name = "TOP MESH";
                }
                previousPerpPoints[0] = pointArr[0];
                previousPerpPoints[1] = pointArr[2];

                previous = hit.point;
            }
        }
    }

    IEnumerator changeSize(float change){
        canChangeSize = false;
        spot.transform.localScale += new Vector3(change, 0.0f, change);
        yield return new WaitForSeconds(cooldown);
        canChangeSize = true;
    }

    float distanceFrom(Vector3 start, Vector3 end){
        double xcalc = Math.Pow((double)end.x - start.x, 2);
        double zcalc = Math.Pow((double)end.z - start.z, 2); 
        return (float)Math.Sqrt(xcalc+zcalc);
    }

    Vector3[] points(Vector3 start, Vector3 end, Vector3 crossDir){
        float size = spot.transform.localScale.x/2;

        Vector3 direction = end - start;
        direction /= direction.magnitude;
        Vector3 perpLine = Vector3.Cross(direction, crossDir).normalized;

        Vector3 point1 = end+size*perpLine;
        Vector3 point2 = start+size*perpLine;
        Vector3 point3 = end-size*perpLine;
        Vector3 point4 = start-size*perpLine;

        //If points aren't the same
        //if(previousPerpPoints[0] != Vector3.up && previousPerpPoints[0] != point1 && previousPerpPoints[1] != point2){
            //Create a mesh between the current and last wall to fill in gap
        //    createMesh(new Vector3[] {previousPerpPoints[0], previousPerpPoints[1], point1, point2});
        //    Debug.Log("Making new mesh");
        //}
        
        
        
        Vector3[] pointsArr = new Vector3[]{point1, point2, point3, point4, perpLine};

        return pointsArr;
    }

    Mesh createMesh(Vector3[] pointsArray, Vector3 normal, int[] tri){
        Mesh mesh = new Mesh {
            name = "Wall Mesh"
        };

        GameObject w = Instantiate(wall);
        w.GetComponent<MeshFilter>().mesh = mesh;

        Vector3 size = (spot.transform.localScale/2).normalized;

        mesh.vertices = new Vector3[] {
           pointsArray[0], pointsArray[1], pointsArray[2], pointsArray[3]
        };

        mesh.normals = new Vector3[] {
            normal, normal, normal, normal
        };

        mesh.triangles = tri;

        return mesh;
    }
}
