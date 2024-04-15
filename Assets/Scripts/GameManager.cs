using System;
using System.Collections;
using System.Collections.Generic;
using FIMSpace.Jiggling;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource whoosh;
    public AudioSource boom;
    public AudioSource grab;
    public AudioSource release;
    public AudioSource splash;
    public AudioSource charge;
    public AudioSource fail;
    public AudioSource success;
    public AudioSource walkaway;
    public AudioSource lightsOn;
    public AudioSource victory;
    public AudioSource click;
    public AudioSource impact;
    
    [Header("Screamer")]
    public AudioSource suspience;
    public AudioSource horror_reveal;
    public AudioSource night_scream;
    public AudioSource stomping;
    public AudioSource metal_ambient;
    public AudioSource heartbeat;

    [Header("Intro management")]
    public GameObject lights;
    public bool introComplete = false;
    public TMP_Text introText;
    private int _lastIntroWord;
    private float _lastIntroWordPlaced;
    public float introSpeed = .3f;
    
    [Header("References")]
    public GameObject hand;
    public Animator pointerAnimator;
    public Camera mainCamera;
    public GameObject handCart;

    public GameObject trackingPoint;
    public Transform lookAtPoint;
    public GameObject grabbingPoint;
    public item_controller[] allItems;
    public Animator cameraAnimator;
    public pot_manager potManager;
    public GameObject spawnEffectPrefab;
    public GameObject spawnEffectPoint;
    public GameObject spawnErrorEffectPrefab;
    public FJiggling_Active camJiggle;
    
     
    [Header("Configurations")]
    public float pointerDistance = 3f;
    public Vector3 handOnGrabRotation = new Vector3(0, 0, 0);
    public float rotationSpeed = 200f;
    public LayerMask grabMask;
    public LayerMask potMask;

    private Vector3 _average;
    private Vector3 _lastPosition;
    private Quaternion _defaultSpawnRotation;
    private bool _isOnScreen;
    private bool _isGrabbing;
    private item_controller _grabbedItem;
    
    
    public int smoothingBufferSize = 10;
    private List<Vector3> _trackerPositions = new List<Vector3>();
    
    [Header("Combinations")]
    public Utilities.SummonStatus summonStatus = Utilities.SummonStatus.Ready;
    public CombinationsMenu combinationsMenu;
    public EntitiesMenu entitiesMenu;
    public FailedEntitiesMenu failedEntitiesMenu;
    public GameObject spawnPoint;
    private static readonly int Observing = Animator.StringToHash("observing");
    
    [Header("Debug")]
    public GameObject summonedObject;
    private CombinationRecords _recordPassed;
    public float timeForObserving;
    public float minTimeForObserving = 3f;
    public Utilities.EntityType lastGoal = Utilities.EntityType.Hawking;
    public bool isRotating;
    private string[] _cachedIntroText;
    private bool _mainIntroComplete;
    public int failsCount;
    public int maxFails = 3;
    
    [Header("Game Flow")]
    public Utilities.EntityType nextGoal = Utilities.EntityType.KnightKitty;
    
    [Header("Typography")]
    public TMP_Text goalText;
    public GameObject helperBlinker;
    public TMP_Text successText;
    public TMP_Text failedText;
    public TMP_Text entityText;

    private static readonly int Walkaway = Animator.StringToHash("walkaway");

    private void PlayOneShot(AudioSource source)
    {
        var sourcePitch = source.pitch;
        var pitch = sourcePitch >= 0.85f ? UnityEngine.Random.Range(.9f, 1.1f) : sourcePitch;
        source.pitch = pitch;
        source.PlayOneShot(source.clip);
    }

    private void Start()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
        }
        
        camJiggle.enabled = false;
        _defaultSpawnRotation = spawnPoint.transform.rotation;
        
        UpdateGoalText(nextGoal);
        
        var sample = "So, another working day at the ritual bureau. My task for today is to summon some very important entities. But I can barely remember the ingredients. Hopefully, I'll finish it soon...";
        _cachedIntroText = sample.Split(' ');
    }

    private void UpdateGoalText(Utilities.EntityType entity)
    {
        var text = $"SUMMON: {entity.ToString()}";
        if (entity == Utilities.EntityType.Random)
        {
            text = "HAVE FUN!";
        }
        UpdateGoalText(text);
    }

    private void UpdateGoalText(string text)
    {
        goalText.text = text;
    }


    private void Update()
    {
        HandleTrackerPosition();
        HandleHandPosition();
        HandleAnimationState();
        ResetHandler();
        RotateRunwayController();
        IntroTextTyper();
    }

    private void IntroTextTyper()
    {
       
        if (!introComplete)
        {
            if (Time.time < 2f) return;
            if (_lastIntroWordPlaced > Time.time) return;
            if (_lastIntroWord >= _cachedIntroText.Length)
            {

                if (!_mainIntroComplete)
                {
                    var controlsText = "\n\n\t\t<color=grey><color=#3EFFC7>[LMB]</color> TO GRAB ITEMS\n\t\t<color=#3EFFC7>[ESC]</color> TO BEGIN</color>";
                    introText.text += controlsText;
                    _mainIntroComplete = true;
                    PlayOneShot(click);
                }

                return;
            }

            // append word by word to the intro text
            var currentText = introText.text;
            
                if (_lastIntroWord < _cachedIntroText.Length)
                {
                    var nextWord = _cachedIntroText[_lastIntroWord];
                    currentText += " " + nextWord;
                    introText.text = currentText;
                    _lastIntroWord++;
                    
                    PlayOneShot(click);
                    
                    var delay = introSpeed;

                    if (nextWord.Contains("."))
                    {
                        delay = introSpeed * 35;
                    }
                    _lastIntroWordPlaced = Time.time + (delay * UnityEngine.Random.Range(.8f, 1.2f));
                }
                
        }
    }

    private void RotateRunwayController()
    {
        var cameraAnimateState = cameraAnimator.GetBool(Observing);
        var mousePressed = Input.GetMouseButton(0) && _grabbedItem == null;
        if (summonStatus == Utilities.SummonStatus.Busy && cameraAnimateState)
        {
            if (mousePressed)
            {
                var rotate = Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed;
                spawnPoint.transform.Rotate(Vector3.up, -rotate);
                if(!isRotating) isRotating = true;
            }
        }
        else
        {
            if (isRotating)
            {
                isRotating = false;
                spawnPoint.transform.rotation = _defaultSpawnRotation;
            }
        }
    }

    private void ResetHandler()
    {
        var cameraAnimateState = cameraAnimator.GetBool(Observing);
        
        if(cameraAnimateState && Input.GetKeyDown(KeyCode.Escape))
        {
            if(timeForObserving > Time.time) return;
            helperBlinker.SetActive(false);
            cameraAnimator.SetBool(Observing, false);
            PlayOneShot(whoosh);
            ResetObjects();
            DisableTexts();
            
            if (!introComplete)
            {
                introText.gameObject.SetActive(false);
                introComplete = true;
                Invoke(nameof(CompleteIntro), 1f);
            }
        }

        if (timeForObserving < Time.time && !helperBlinker.activeInHierarchy && summonStatus == Utilities.SummonStatus.Busy)
        {
            helperBlinker.SetActive(true);
        }
    }
    
    private void CompleteIntro()
    {
        PlayOneShot(lightsOn);
        lights.SetActive(true);
    }

    private void ResetObjects()
    {
        // reset all indicators
        potManager.ResetIndicators();
        UpdateGoalText(nextGoal);
        potManager.ResetAllItems();
        entityText.text = "";
        
        // enable all items
        foreach (var item in allItems)
        {
            item.myParent.SetActive(true);
        }

        if (summonedObject)
        {
            // destroy summoned object
            var summonedAnimator = summonedObject.GetComponent<Animator>();
            summonedObject.transform.localEulerAngles += new Vector3(0f, 55f, 0f);
            PlayOneShot(walkaway);
            if (summonedAnimator)
            {
                summonedAnimator.SetBool(Walkaway, true);
            }

            // destroy summoned object after 5 seconds
            Destroy(summonedObject, 5f);
            summonedObject = null;
        }

        // reset summon status
        UpdateSummonStatus(Utilities.SummonStatus.Ready);
        _recordPassed = null;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void HandleAnimationState()
    {
        if(Input.GetMouseButtonDown(0) && !_isGrabbing)
        {
            pointerAnimator.SetBool("Natural", false);
            pointerAnimator.SetBool("GrabLarge", true);
            _isGrabbing = true;
            GrabCheck();
        }
        else if((Input.GetMouseButtonUp(0) || !_isOnScreen) && _isGrabbing)
        {
            pointerAnimator.SetBool("GrabLarge", false);
            pointerAnimator.SetBool("Natural", true);
            _isGrabbing = false;
            
            if (!_grabbedItem) return;
            
            
            var potDetected = PotDetector();
            if (potDetected && summonStatus == Utilities.SummonStatus.Ready)
            {
                PlayOneShot(splash);
                _grabbedItem.PlacedToPot();
                potDetected.PutItem(_grabbedItem);
            } else
            {
                PlayOneShot(release);
                _grabbedItem.Release();
            }
            
            MoveHandWhenGrabbing(false);
            _grabbedItem = null;
        }
    }

    private pot_manager PotDetector()
    {
        var from = mainCamera.transform.position;
        // forward from camera
        var direction = trackingPoint.transform.position - from;
        Ray ray = new Ray(from, direction);
        RaycastHit hit;
        
        var layerMaskInt = potMask.value;
        
        if (Physics.SphereCast(ray, .1f, out hit, 20f, layerMaskInt))
        {
            if (hit.collider.CompareTag("pot"))
            {
                var potController = hit.collider.GetComponent<pot_manager>();
                return potController;
            }
        }
        return null;
    }

    private void GrabCheck()
    {
        var from = mainCamera.transform.position;
        // forward from camera
        var direction = trackingPoint.transform.position - from;
        Ray ray = new Ray(from, direction);
        RaycastHit hit;

        var layerMaskInt = grabMask.value;
        
        
        if (Physics.SphereCast(ray, .1f, out hit, 20f, layerMaskInt))
        {
            if (hit.collider.CompareTag("grabbable") && !_grabbedItem)
            {
                var hitGrabController = hit.collider.GetComponent<item_controller>();
                if (hitGrabController)
                {
                    PlayOneShot(grab);
                    MoveHandWhenGrabbing(true);
                    hitGrabController.Grab(grabbingPoint);
                    _grabbedItem = hitGrabController;
                }
            }
        }
    }

    private void MoveHandWhenGrabbing(bool b)
    {
        if (b)
        {
            handCart.transform.localEulerAngles = handOnGrabRotation;
        } else {
            handCart.transform.localEulerAngles = Vector3.zero;
        }
    }

    private void HandleHandPosition()
    {
        // hand is following the pointer
        if (_trackerPositions.Count > 0)
        {
            hand.transform.position = _average;
        }
        
        // rotated towards camera
        Vector3 direction = lookAtPoint.transform.position - hand.transform.position;
        hand.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void HandleTrackerPosition()
    {
       // make 3d hand following the pointer on screen projection
       Vector3 mousePosition = Input.mousePosition;
       mousePosition.z = pointerDistance;
       Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
       
       // check if the pointer out of screen then set it to the last position
       var screenPoint = mainCamera.WorldToViewportPoint(worldPosition);
       _isOnScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
       
       if (!_isOnScreen)
       {
           worldPosition = _lastPosition;
       }
       else
       {
           _lastPosition = worldPosition;
       }
       
       trackingPoint.transform.position = worldPosition;
       _trackerPositions.Add(worldPosition);
       _average += worldPosition / smoothingBufferSize;

       if (_trackerPositions.Count > smoothingBufferSize)
       {
           _trackerPositions.RemoveAt(0);
           _average -= _trackerPositions[0] / smoothingBufferSize;
       }
    }

    public void StartSummoning(List<item_controller> items)
    {
        if(summonStatus != Utilities.SummonStatus.Ready) return;
        UpdateSummonStatus(Utilities.SummonStatus.Busy);
        camJiggle.enabled = true;
        
        _recordPassed = null;
        var combinations = combinationsMenu.combinations;
        var itemOne = items[0].elementType;
        var itemTwo = items[1].elementType;
        var itemThree = items[2].elementType;

        for (int i = 0; i < combinations.Count; i++)
        {
            var recipe = combinations[i].recipe;
            var list = new List<Utilities.ElementsType>
            {
                recipe.itemOne,
                recipe.itemTwo,
                recipe.itemThree
            };
            
            if (list.Contains(itemOne) && list.Contains(itemTwo) && list.Contains(itemThree))
            {
                _recordPassed = combinations[i];
                break;
            }
        }

        if (_recordPassed == null)
        {
            failsCount++;
            
            if (failsCount >= maxFails)
            {
                failsCount = maxFails;
            }
            
        }
        else
        {
            PlayOneShot(charge);
        }
 
        
        timeForObserving = Time.time + minTimeForObserving;
        Invoke(nameof(SpawnStageOne), 1f);
    }

    private void SpawnStageOne()
    {
        if (_recordPassed == null)
        {
            timeForObserving -= minTimeForObserving / 2f;
            Debug.Log("Failed to summon");
            var fx = Instantiate(spawnErrorEffectPrefab, spawnEffectPoint.transform.position, Quaternion.identity);
            
            var chancesLeft = maxFails - failsCount;

            if (chancesLeft <= 0)
            {
                PlayOneShot(impact);
            }
        }
        else
        {
            Debug.Log("Summoned: " + _recordPassed.result.ToString());
            var toSpawn = entitiesMenu.entities.Find(x => x.type == _recordPassed.result).prefab;
            if (toSpawn)
            {
                PlayOneShot(boom);
                StopJigglingCamera();
                var fx = Instantiate(spawnEffectPrefab, spawnEffectPoint.transform.position, Quaternion.identity);
                var spawned = Instantiate(toSpawn, spawnPoint.transform.position, Quaternion.Euler(Vector3.zero));
                spawned.transform.SetParent(spawnPoint.transform);
                spawned.transform.localRotation = Quaternion.Euler(Vector3.zero);
                summonedObject = spawned;
            }
        }

        Invoke(nameof(SpawnStageTwo), .5f);
    }

    private void EnableHelperAttentionJiggler()
    {

        CombinationRecords record = null;
        // nextGoal item
        foreach (var item in combinationsMenu.combinations)
        {
            Debug.Log($"Item: {item.result}");
            if (item.result != nextGoal) continue;
            record = item;
            break;
        }
        
        Debug.Log($"Next goal: {nextGoal}, found: {record}");
        
        if(record == null) return;

        var count = 0;
        foreach (var item in allItems)
        {
            if(count >= failsCount) break;
            if (item.elementType == record.recipe.itemOne || item.elementType == record.recipe.itemTwo || item.elementType == record.recipe.itemThree)
            {
                if (!item.attentionJiggle)
                {
                    item.ActivateAttentionJiggler();
                }

                count++;
            }
        }
    }

    private void NextGoalProcess()
    {
        EntitySuccess();
        UpdateGoalText("");
        ResetAttentionJigglers();
        var nextEntityGoal = nextGoal + 1;
        if (nextEntityGoal > lastGoal)
        {
            nextEntityGoal = Utilities.EntityType.Random;
        }
        nextGoal = nextEntityGoal;
        failsCount = 0;
    }

    private void ResetAttentionJigglers()
    {
        foreach (var item in allItems)
        {
            item.DeactivateAttentionJiggler();
        }
    }

    private void EntitySuccess()
    {
        PlayOneShot(success);
        failedText.gameObject.SetActive(false);
        var text = $"NICE! I'VE SUMMONNED {nextGoal.ToString().ToUpper()}! \nWHO'S UP NEXT?";

        if (nextGoal == lastGoal)
        {
            text = "WOW! I'VE SUMMONNED ALL THE ENTITIES! \nNOW I CAN DO WHATEVER I WANT!";
            Invoke(nameof(VictorySound), 1f);
        }
        
        successText.text = text;
        successText.gameObject.SetActive(true);
    }

    private void VictorySound()
    {
        PlayOneShot(victory);
    }
    
    private void NotThiEntity()
    {
        failedText.gameObject.SetActive(true);
        failedText.text = $"SNAP! THAT'S NOT A {nextGoal.ToString().ToUpper()}! \nI SHOULD TRY AGAIN...";
        successText.gameObject.SetActive(false);
    }
    
    private void NothingToSummon()
    {
        var chancesLeft = maxFails - failsCount;
        PlayOneShot(fail);
        var text = $"DAMN! NOTHING WAS SUMMONED! \nGOTTA BE CAREFUL, {chancesLeft} CHANCES LEFT...";

        if (chancesLeft <= 0)
        {
            text = "I'VE USED ALL MY CHANCES... \nUNKNOWN ENTITY IS COMING...";
        }
        
        failedText.gameObject.SetActive(true);
        failedText.text = text;
        successText.gameObject.SetActive(false);
    }

    private void DisableTexts()
    {
        failedText.gameObject.SetActive(false);
        successText.gameObject.SetActive(false);
    }
    
    private void SpawnStageTwo()
    {
        cameraAnimator.SetBool(Observing, true);
        PlayOneShot(whoosh);
        if (_recordPassed == null)
        {
            StopJigglingCamera();
            NothingToSummon();
            EnableHelperAttentionJiggler();
        }
        else
        {
            entityText.text = _recordPassed.result.ToString().ToUpper();
            if (nextGoal != Utilities.EntityType.Random)
            {
                if (nextGoal == _recordPassed.result)
                {
                    NextGoalProcess();
                }
                else
                {
                    NotThiEntity();
                }
            }
        }
    }

    private void StopJigglingCamera()
    {
        camJiggle.enabled = false;
    }

    private void UpdateSummonStatus(Utilities.SummonStatus newStatus)
    {
        summonStatus = newStatus;
    }
}
