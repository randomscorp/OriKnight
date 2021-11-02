using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Modding;
using GlobalEnums;
using System.Collections;

namespace OriKnight.Utils
{
    class ShurikenBehaviour: MonoBehaviour
    {
       
        public float speed = 25;
        private bool foward = true;
        private Rigidbody2D body;
        float time=0;
       
        public states currentState = states.Foward;
        private states prevState;
        
        public float fowardTime=0.4f;
        public float hangTime = 0.2f;

        public Vector2 direction = new(1, 0);

        public enum states
        {
            Foward=0,
            Hang=1,
            Back=2
        }


        void OnCollisionEnter2D(Collision2D collision)
        {
            //Modding.Logger.Log(PlayMakerFSM.)

            if (collision.collider.gameObject.layer == ((int)PhysLayers.TERRAIN)||
                collision.collider.gameObject.layer == ((int)PhysLayers.ENEMIES))
            {
              Physics2D.IgnoreCollision(collision.collider,collision.otherCollider);
            }


        }

        void OnTriggerEnter2D(Collider2D col)
        {
           
            if(currentState== states.Back && col.gameObject.name == "HeroBox")
            {
                direction = new Vector2(1, 0);
                HeroController.instance.gameObject.SendMessage("ShurikenCD");
                Destroy(this.gameObject);

            }
        }



        void Start()
        {
           
            body = this.GetComponent<Rigidbody2D>();
            time = 0;
            prevState = currentState;
        }

        private void StateChange(states currentState,states prevState)
        {
            prevState = currentState;
        }

        void FixedUpdate()
        {
            if (currentState != prevState) StateChange(currentState,prevState);
            switch (currentState)
            {               
                case states.Foward:

                    body.velocity = direction.normalized*speed;
                    time += Time.fixedDeltaTime;
                    if (time>= fowardTime) { time = 0; currentState = states.Hang; }
                    break;
                    
                case states.Hang:
                    
                    body.velocity = direction*speed/10;
                    time += Time.fixedDeltaTime;
                    if (time >= hangTime) { time = 0; currentState = states.Back; }
                    break;

                case states.Back:

                   // if () { Destroy(this.gameObject); break; }
                    speed = Math.Abs(speed);
                    body.velocity = (body.transform.position - HeroController.instance.transform.position).normalized * -speed;
                    //HeroController.instance.playerData.
                    
                    
                    break;
            }
                      
        }

        


    }
}
