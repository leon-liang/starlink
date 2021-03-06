﻿using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class Enemy_II : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    
    private CharacterController _controller;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    private Vector3 _velocity;
    private Vector3 _initialPosition;

    private bool _isGrounded;

    private Transform _playerTransform;
    private Player _player;

    public Image healthBar;
    public GameObject healthBarGameObject;

    // Start is called before the first frame update
    
    private float explosionForce = 100f;
    private float explosionRadius = 4f;
    private float explosionUpward = 1f;

    private Coroutine bounceRoutine;
    private Coroutine landRoutine;

    private float cubeSize = 0.2f;
    private float cubesInRow = 5;

    private float _cubesPivotDistance;
    private Vector3 _cubesPivot;

    [SerializeField] private Material enemyMaterial;
    
    private bool _isBouncing = false;
    void Start()
    {
        _controller = gameObject.GetComponent<CharacterController>();
        GameObject playerGameobject = GameObject.Find("Player");
        _playerTransform = playerGameobject.GetComponent<Transform>();
        _player = playerGameobject.GetComponent<Player>();
        _controller = gameObject.GetComponent<CharacterController>();
        
        _cubesPivotDistance = cubeSize * cubesInRow / 2;
        _cubesPivot = new Vector3(_cubesPivotDistance, _cubesPivotDistance, _cubesPivotDistance);
        
        StartCoroutine(SetInitialPosition());
    }

    // Update is called once per frame
    void Update()
    {
        if (health == 100f)
        {
            healthBarGameObject.SetActive(false);
        }
        else
        {
            healthBarGameObject.SetActive(true);
        }
        
        if (health < 0)
        {
            FindObjectOfType<AudioManager>().Play("Smashing");
            Destroy(this.gameObject);
            for (int x = 0; x < cubesInRow; x++)
            {
                for (int y = 0; y < cubesInRow; y++)
                {
                    for (int z = 0; z < cubesInRow; z++)
                    {
                        CreatePiece(x, y, z);
                    }
                }
            }

            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, explosionUpward);
                }
            }
        }
        CalculateMovement();

        StartCoroutine(CheckIfGrounded());
        
        if (_playerTransform)
        {
            float dist = Vector3.Distance(_playerTransform.position, transform.position);

            if (dist < 2.5f && !_isBouncing)
            {
                _isBouncing = true;
                FindObjectOfType<AudioManager>().Play("Bounce");
                CharacterController playerController = _player.GetComponent<CharacterController>();
                
                StartCoroutine(StopBounceback());
                bounceRoutine = StartCoroutine(BounceBack(playerController));
                StartCoroutine(StopLandRoutine());
                landRoutine = StartCoroutine(LandRoutine(playerController));
            }
        }
    }

    IEnumerator BounceBack(CharacterController playerController)
    {
        while (true)
        {
            playerController.Move(_player.transform.TransformDirection(new Vector3(0, 13f, -11)) * Time.deltaTime);
            yield return 0;
        }
       
    }

    IEnumerator StopLandRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        _isBouncing = false;
        StopCoroutine(landRoutine);
    }

    IEnumerator LandRoutine(CharacterController playerController)
    {
        while (true)
        {
            playerController.Move(_player.transform.TransformDirection(new Vector3(0, 0f, -11)) * Time.deltaTime);
            yield return 0;
        }
    }

    IEnumerator StopBounceback()
    {
        yield return new WaitForSeconds(0.5f);
        StopCoroutine(bounceRoutine);
    }
    
    

    IEnumerator CheckIfGrounded()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (_initialPosition != transform.position)
            {
                health -= 5;
                healthBar.fillAmount = health / 100f;
                _initialPosition = transform.position;
            }
        }
    }

    IEnumerator SetInitialPosition()
    {
        yield return new WaitForSeconds(0.5f);
        _initialPosition = transform.position;

    }
    
    void CreatePiece(int x, int y, int z)
    {
        GameObject piece;
        piece = GameObject.CreatePrimitive(PrimitiveType.Cube);

        piece.transform.position = transform.position + new Vector3(cubeSize * x, cubeSize * y, cubeSize * z) - _cubesPivot;
        piece.transform.localScale = new Vector3(cubeSize, cubeSize ,cubeSize);
        piece.GetComponent<Renderer>().material = enemyMaterial;
        
        piece.AddComponent<Rigidbody>();
        piece.GetComponent<Rigidbody>().mass = cubeSize;
        piece.transform.tag = "Projectile";
        // piece.GetComponent<BoxCollider>().isTrigger = true;
        Destroy(piece, UnityEngine.Random.Range(2f, 3f));
    }

    void CalculateMovement()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += gravity * Time.deltaTime;
        
        _controller.Move(_velocity * Time.deltaTime);
    }
}
