using UnityEngine;

public class RopeSpawn : MonoBehaviour
{
    [SerializeField]
    GameObject partPrefab, parentObject;
    [Range(0, 1000)]
    int length = 1;

    [SerializeField]
    float partDistance = 0.21f;
    
    [SerializeField]
    bool reset, spawn, snapfirst, snaplast;

    // Update is called once per frame
    void Update()
    {
        if (reset)
        {
            foreach (GameObject tmp in GameObject.FindGameObjectsWithTag("Player"))
            {
                Destroy(tmp);
            }
        }
    }

    public void Spawn 
}
