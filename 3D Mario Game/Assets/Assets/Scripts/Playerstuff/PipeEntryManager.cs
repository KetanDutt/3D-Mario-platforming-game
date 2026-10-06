using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeEntryManager : MonoBehaviour
{
    public Player player;

    private Transform pipe;
    private Transform EnoughGoDown;
    private Rigidbody rb;
    private Vector3 direction;

    private bool downpipe = false;
    private bool upPipe = false;
    private bool scene_change = false;
    private bool scene_return = false;
    private bool scene_change_stillMoving = false;

    private Pipe pipe_script;
    private int pipeid = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (player == null) player = GetComponent<Player>();
    }

    void Update()
    {
        if (pipe != null)
        {
            direction = pipe.position - transform.position;

            if (downpipe)
            {
                bool went_down_pipe = false;

                // Move towards pipe center
                if ((Mathf.Abs(pipe.position.x - transform.position.x) >= 0.05f || Mathf.Abs(pipe.position.z - transform.position.z) >= 0.05f) &&
                    (EnoughGoDown == null || transform.position.y - EnoughGoDown.position.y >= 0.5f))
                {
                    player.PipeEntry = true;
                    rb.velocity = new Vector3(direction.x * 5f, 0f, direction.z * 5f);
                }
                else if (EnoughGoDown == null || transform.position.y - EnoughGoDown.position.y >= 0.5f)
                {
                    rb.isKinematic = true;
                    transform.Translate(0, -3f * Time.deltaTime, 0, Space.World);
                }

                // After going down enough, teleport to destination pipe
                if (EnoughGoDown != null && transform.position.y - EnoughGoDown.position.y < 0.5f && !went_down_pipe)
                {
                    went_down_pipe = true;
                    downpipe = false;

                    if (pipe_script != null && pipe_script.destination_Pipe != null)
                    {
                        if (pipe_script.destination_Pipe.transform.childCount > 1)
                        {
                            player.transform.position = pipe_script.destination_Pipe.transform.GetChild(1).position;
                        }
                        if (Camera.main != null && Camera.main.transform.parent != null && pipe_script.destination_Pipe.transform.childCount > 0)
                        {
                            Camera.main.transform.parent.position = pipe_script.destination_Pipe.transform.GetChild(0).position;
                            Camera.main.transform.parent.rotation = pipe_script.destination_Pipe.transform.GetChild(0).rotation;
                        }
                    }
                }
            }

            if (upPipe && pipe_script != null && pipe_script.destination_Pipe != null && pipe_script.destination_Pipe.transform.childCount > 2)
            {
                Transform topExit = pipe_script.destination_Pipe.transform.GetChild(2);
                if (Vector3.Distance(topExit.position, transform.position) > 0.1f)
                {
                    transform.Translate(0, 3f * Time.deltaTime, 0, Space.World);
                }
                else
                {
                    upPipe = false;
                    rb.isKinematic = false;
                    player.PipeEntry = false;
                }
            }

            if (!downpipe && !upPipe && !rb.isKinematic)
            {
                player.PipeEntry = false;
            }
        }
    }

    IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pipe") && !player.MEGAMUSHROOM && !player.holdingShell)
        {
            Pipe pComp = other.GetComponent<Pipe>();
            if (pComp != null)
            {
                pipe = other.transform.childCount > 0 ? other.transform.GetChild(0) : other.transform;
                EnoughGoDown = other.transform.childCount > 1 ? other.transform.GetChild(1) : null;
                pipe_script = pComp;
                pipeid = pComp.Pipe_ID;

                // Wait for player to stand on pipe and press V
                float waitTimeout = 2.5f;
                while (!Input.GetKeyDown(KeyCode.V) && waitTimeout > 0f && Vector3.Distance(pipe.position, transform.position) < 1.8f)
                {
                    waitTimeout -= Time.deltaTime;
                    yield return null;
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    downpipe = true;
                    player.PipeEntry = true;
                    AudioSource pipeAud = other.GetComponentInParent<AudioSource>();
                    if (pipeAud != null) pipeAud.Play();

                    if (pipeid == 1) { scene_change = true; scene_return = false; }
                    else if (pipeid == 3 || pipeid == 5) { scene_change = false; scene_return = true; }
                    else if (pipeid == 4) { scene_change_stillMoving = true; scene_return = false; }
                }
            }
        }

        if (other.CompareTag("PipeDestination"))
        {
            upPipe = true;
            yield return new WaitForSeconds(0.6f);

            if (pipe != null)
            {
                AudioSource pAud = pipe.GetComponentInParent<AudioSource>();
                if (pAud != null) pAud.Play();
            }

            // Underground music transition (Fixed infinite loop!)
            if (scene_change || scene_change_stillMoving)
            {
                if (Camera.main != null && Camera.main.transform.parent != null)
                {
                    CameraFollow cf = Camera.main.transform.parent.GetComponent<CameraFollow>();
                    if (cf != null) cf.Underground = true;
                }

                AudioSource surfaceAud = Camera.main != null ? Camera.main.GetComponent<AudioSource>() : null;
                if (surfaceAud != null)
                {
                    while (surfaceAud.volume > 0.05f)
                    {
                        surfaceAud.volume -= 0.1f;
                        yield return new WaitForSeconds(0.02f);
                    }
                    surfaceAud.Stop();
                }

                if (Camera.main != null && Camera.main.transform.childCount > 2)
                {
                    AudioSource caveAud = Camera.main.transform.GetChild(2).GetComponent<AudioSource>();
                    if (caveAud != null) caveAud.Play();
                }
                scene_change = false;
                scene_change_stillMoving = false;
            }
            else if (scene_return)
            {
                if (Camera.main != null && Camera.main.transform.parent != null)
                {
                    CameraFollow cf = Camera.main.transform.parent.GetComponent<CameraFollow>();
                    if (cf != null) cf.Underground = false;
                }

                // Fade out cave music (Fixed bug: was volume < 0.6 causing infinite loop!)
                if (Camera.main != null && Camera.main.transform.childCount > 2)
                {
                    AudioSource caveAud = Camera.main.transform.GetChild(2).GetComponent<AudioSource>();
                    if (caveAud != null)
                    {
                        while (caveAud.volume > 0.05f)
                        {
                            caveAud.volume -= 0.1f;
                            yield return new WaitForSeconds(0.02f);
                        }
                        caveAud.Stop();
                    }
                }

                // Restore surface music
                AudioSource surfaceAud = Camera.main != null ? Camera.main.GetComponent<AudioSource>() : null;
                if (surfaceAud != null)
                {
                    surfaceAud.volume = 0.6f;
                    surfaceAud.Play();
                }
                scene_return = false;
            }
        }
    }
}
