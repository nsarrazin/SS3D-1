using UnityEngine;
using Mirror;


namespace SS3D.Content.Systems.Player
{
    public class Aiming : NetworkBehaviour
    {
        private new Camera camera;
        public bool isAiming = false;
        [SyncVar] public float rotationSpeed = 25f;

        [SerializeField] PointAt[] PointAts;
        // Start is called before the first frame update
        void Start()
        {
            camera = CameraManager.singleton.playerCamera;

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(1) && Input.GetButton("Examine"))
            {
                isAiming = true;
            }

            if (Input.GetMouseButtonUp(1))
            {
                isAiming = false;
            }

            if (isAiming)
            {
                Ray cameraRay = camera.ScreenPointToRay(Input.mousePosition);
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                float rayLength;

                if (groundPlane.Raycast(cameraRay, out rayLength))
                {
                    Vector3 pointToLook = cameraRay.GetPoint(rayLength);
                    Debug.DrawLine(transform.position, pointToLook, Color.cyan);


                    Vector3 deltaLook = pointToLook - transform.position;
                    deltaLook.y = 0.0f;
                    Quaternion lookOnLook = Quaternion.LookRotation(deltaLook);

                    transform.rotation = Quaternion.Slerp(transform.rotation, lookOnLook, rotationSpeed * Time.deltaTime);

                    camera.transform.position = Vector3.Lerp(camera.transform.position,
                                                                    new Vector3(50, 0, 10),
                                                                    10 * Time.deltaTime); ;
                }

            }


        }
    }
}
