using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Mouse_move : MonoBehaviour
{

    //　レイを飛ばす距離
    private float rayRange = 100f;
    //　移動する位置
    private Vector3 targetPosition;
    //　速度
    private Vector3 velocity;
    //　移動スピード
    [SerializeField]
    private float moveSpeed = 1.5f;
    //　マウスクリックで移動する位置を決定するかどうか
    [SerializeField]
    private bool isMouseDownMode = true;
    //　スムースにキャラクターの向きを変更するかどうか
    [SerializeField]
    private bool smoothRotateMode = true;
    //　回転度合い
    [SerializeField]
    private float smoothRotateSpeed = 180f;
    private CharacterController characterController;
    private Animator animator;

    private GameObject lookTarget;

    private bool TargetFlag= true;

    public float timecount = 0.0f;

    private bool HitouchFlag = false;

    public float value = 0;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        targetPosition = transform.position;
        velocity = Vector3.zero;
        lookTarget = GameObject.FindWithTag("Me");
        Debug.Log(lookTarget);
    }

    void Update()
    {
        AnimatorStateInfo anim = animator.GetCurrentAnimatorStateInfo(0);

        velocity = Vector3.zero;
        //　マウスクリックまたはisMouseDownModeがOffの時マウスの位置を移動する位置にする
        if (Input.GetButton("Fire1") || !isMouseDownMode)
        {
            timecount = 0;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction*rayRange, Color.green, 5, false);

            if (Physics.Raycast(ray, out hit, rayRange, LayerMask.GetMask("Field")) && Input.GetButton("Fire1"))
            {
                targetPosition = hit.point;
                HitouchFlag = false;
            }
        }
        //　移動の目的地と0.1mより距離がある時は速度を計算
        if (Vector3.Distance(transform.position, targetPosition) > 0.7f)
        {
            timecount = 0;
            TargetFlag = false;
            var moveDirection = (targetPosition - transform.position).normalized;
          
            velocity = new Vector3(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed, 0);
            //　スムースモードの時は徐々にキャラクターの向きを変更する
            if (smoothRotateMode)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new Vector3(moveDirection.x, 0, 0)), smoothRotateSpeed * Time.deltaTime);
                //　スムースモードでなければ一気に目的地の方向を向かせる
            }
            else
            {
                transform.LookAt(transform.position + new Vector3(moveDirection.x, 0, 0));
            }
            //　アニメーションパラメータの設定
            animator.SetFloat("Speed", moveDirection.magnitude);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
            if (lookTarget)
            {
                Quaternion lockRotation = Quaternion.LookRotation(lookTarget.transform.position - transform.position, Vector3.up);

                
                lockRotation.x = 0;
                lockRotation.z = 0;

                transform.rotation = Quaternion.Slerp(transform.rotation, lockRotation, 8f*Time.deltaTime);

                
                if(Mathf.Abs((transform.rotation * Quaternion.Inverse(lockRotation)).w) > 0.95)
                {
                    TargetFlag = true;
                    if (anim.IsName("Idle"))
                    {
                        timecount += Time.deltaTime;
                    }
                }
            }

            if (HitouchFlag)
            {
                //SceneManager.LoadScene("Hitouch");
            }

            if (timecount >= 15 + value && anim.IsName("Idle"))
            {
                int waitstate = Random.Range(0, 3);

                switch (waitstate)
                {
                    case 0:
                        animator.SetBool("Rest", true);
                        break;
                    case 1:
                        animator.SetBool("JUMP01", true);
                        break;
                    case 2:
                        animator.SetBool("WAIT02", true);
                        break;
                }
                //Debug.Log(waitstate);
                timecount = 0;
                value = Random.Range(0, 15);
            }
            else
            {
                animator.SetBool("Rest", false);
                animator.SetBool("JUMP01", false);
                animator.SetBool("WAIT02", false);
            }
        }

        characterController.Move(velocity * Time.deltaTime);
    }
    void OnAnimatorIK()
    {
        if (TargetFlag)
        {
            animator.SetLookAtWeight(0.5f, 0.3f, 0.5f, 1f, 0.5f);
            animator.SetLookAtPosition(lookTarget.transform.position);
        }
    }
}