using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets & Offsets")]
    public Transform Player;
    public Transform Lookat;
    public Vector3 offset = new Vector3(0, -4, 10.5f);
    public bool UseOffsetValues;
    public Player player_script;

    [Header("Orbit & Sensitivity")]
    public float rotateSpeed = 2.5f;
    private Quaternion rotation;

    [Header("Environment State")]
    public Vector3 Cam_distance;
    public bool Underground = false;
    public bool cloudPlatforms = false;
    public bool startLvel = false;
    public bool restOfCaveLevel = false;
    public bool endOfCaveLevel = false;

    [Header("Level End")]
    private Transform CameraEndPos;
    [HideInInspector]
    public bool CameraLevelEndMovement = false;
    private bool endLevelMusicScheduled = false;
    private bool EndMusicBool = true;
    private GameObject course_clear_text;

    // Persistent SmoothDamp velocity vectors
    private Vector3 offsetSmoothVelocity;
    private Vector3 camPosSmoothVelocity;

    void Start()
    {
        if (Player != null && player_script == null)
        {
            player_script = Player.GetComponent<Player>();
        }

        GameObject endPosObj = GameObject.FindGameObjectWithTag("CameraEndPosition");
        if (endPosObj != null) CameraEndPos = endPosObj.transform;

        course_clear_text = GameObject.FindGameObjectWithTag("CourseClearText");

        if (rotateSpeed <= 0.01f)
        {
            rotateSpeed = 2.5f;
        }
    }

    void LateUpdate()
    {
        if (Player == null || Lookat == null) return;
        if (player_script == null) player_script = Player.GetComponent<Player>();

        if (player_script != null && !player_script.PipeEntry && !Underground && !player_script.flagpole_end)
        {
            // Mouse orbit around target
            if (!player_script.REACHED_GOAL)
            {
                float horizontal = Input.GetAxis("Mouse X") * rotateSpeed;
                Lookat.Rotate(0, horizontal, 0, Space.World);
            }

            // Target camera position based on rotation and offset
            float desiredYangle = Lookat.eulerAngles.y;
            rotation = Quaternion.Euler(0, desiredYangle, 0);
            Vector3 targetPosition = Lookat.position - (rotation * offset);

            // Smooth camera follow
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref camPosSmoothVelocity, 0.05f);
            transform.LookAt(Lookat);
        }

        if (CameraLevelEndMovement)
        {
            LevelEndCameraPositionFunc();
            if (!endLevelMusicScheduled)
            {
                endLevelMusicScheduled = true;
                Invoke("EndLevelMusicFunc", 0.75f);
            }
        }

        // Camera distance adjustments
        if (Input.GetKey(KeyCode.UpArrow))
        {
            offset.y = Mathf.Lerp(offset.y, -6f, 5f * Time.deltaTime);
            offset.z = Mathf.Lerp(offset.z, 15f, 5f * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            offset.y = Mathf.Lerp(offset.y, -3f, 5f * Time.deltaTime);
            offset.z = Mathf.Lerp(offset.z, 10.5f, 5f * Time.deltaTime);
        }

        // Cave zones camera offsets
        if (cloudPlatforms)
        {
            offset = Vector3.SmoothDamp(offset, new Vector3(offset.x, -3f, offset.z), ref offsetSmoothVelocity, 0.2f);
        }
        else if (startLvel)
        {
            offset = Vector3.SmoothDamp(offset, new Vector3(offset.x, -4f, offset.z), ref offsetSmoothVelocity, 0.2f);
        }
        else if (endOfCaveLevel)
        {
            offset = Vector3.SmoothDamp(offset, new Vector3(offset.x, -6.5f, offset.z), ref offsetSmoothVelocity, 0.2f);
        }
        else if (restOfCaveLevel)
        {
            offset = Vector3.SmoothDamp(offset, new Vector3(offset.x, -5f, offset.z), ref offsetSmoothVelocity, 0.2f);
        }
    }

    public void LevelEndCameraPositionFunc()
    {
        if (CameraEndPos != null)
        {
            transform.position = Vector3.Lerp(transform.position, CameraEndPos.position, 5f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, CameraEndPos.rotation, 4f * Time.deltaTime);
        }
    }

    void EndLevelMusicFunc()
    {
        if (EndMusicBool)
        {
            EndMusicBool = false;
            AudioSource aud = GetComponent<AudioSource>();
            if (aud != null) aud.Play();

            if (course_clear_text != null)
            {
                Animator anim = course_clear_text.GetComponent<Animator>();
                if (anim != null) anim.SetBool("CourseClearText", true);
            }
        }
    }
}
