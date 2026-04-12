using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

/// <summary>
/// Unity IAP (Google Play / App Store). Lives on the same DontDestroyOnLoad object as <see cref="DataManager"/>.
/// Configure product ids to match your store listings exactly (consumable gem / coin packs).
/// </summary>
[DefaultExecutionOrder(-50)]
public class IapManager : MonoBehaviour, IDetailedStoreListener
{
    public static IapManager Instance { get; private set; }

    [Serializable]
    public struct ConsumableProduct
    {
        [Tooltip("Product id in Google Play Console / App Store Connect")]
        public string storeProductId;
        [Tooltip("Currency granted to the in-game wallet after a successful purchase.")]
        public int grantedAmount;
        [Tooltip("Fallback display price shown before the store returns localized metadata.")]
        public string fallbackDisplayPrice;
    }

    [SerializeField] List<ConsumableProduct> consumableProducts = new List<ConsumableProduct>
    {
        new ConsumableProduct
        {
            storeProductId = "com.jnijilto.horsetace.gems_small",
            grantedAmount = 100,
            fallbackDisplayPrice = "$0.99"
        },
        new ConsumableProduct
        {
            storeProductId = "com.jnijilto.horsetace.gems_medium",
            grantedAmount = 500,
            fallbackDisplayPrice = "$4.99"
        },
        new ConsumableProduct
        {
            storeProductId = "com.jnijilto.horsetace.gems_large",
            grantedAmount = 1200,
            fallbackDisplayPrice = "$9.99"
        },
        new ConsumableProduct
        {
            storeProductId = "com.jnijilto.horsetace.gems_xlarge",
            grantedAmount = 2500,
            fallbackDisplayPrice = "$19.99"
        },
    };

    /// <summary>Fired after currency is granted and saved (main-thread).</summary>
    public event Action PurchaseSucceeded;

    IStoreController storeController;
    IExtensionProvider extensionProvider;
    DataManager dataManager;
    bool isInitialized;
    readonly Dictionary<string, int> grantByProductId = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        dataManager = GetComponent<DataManager>();
        if (dataManager == null)
        {
            dataManager = FindObjectOfType<DataManager>();
        }

        if (dataManager == null)
        {
            Debug.LogError("[IAP] DataManager not found; IAP disabled.");
            return;
        }

        BuildGrantMap();
        InitializePurchasing();
    }

    void BuildGrantMap()
    {
        grantByProductId.Clear();
        foreach (ConsumableProduct p in consumableProducts)
        {
            if (string.IsNullOrEmpty(p.storeProductId) || p.grantedAmount <= 0)
            {
                continue;
            }
            grantByProductId[p.storeProductId] = p.grantedAmount;
        }
    }

    void InitializePurchasing()
    {
        if (isInitialized)
        {
            return;
        }

        if (!IsIapRuntimeSupported())
        {
            Debug.LogWarning("[IAP] Purchasing not supported on this build target; IAP init skipped.");
            return;
        }

        try
        {
            StandardPurchasingModule module = StandardPurchasingModule.Instance();
#if UNITY_EDITOR
            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
            foreach (ConsumableProduct p in consumableProducts)
            {
                if (string.IsNullOrEmpty(p.storeProductId))
                {
                    continue;
                }
                builder.AddProduct(p.storeProductId, ProductType.Consumable);
            }
            UnityPurchasing.Initialize(this, builder);
        }
        catch (Exception e)
        {
            Debug.LogError("[IAP] Initialize failed: " + e.Message);
        }
    }

    static bool IsIapRuntimeSupported()
    {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return false;
#endif
    }

    public bool IsReady()
    {
        return isInitialized && storeController != null;
    }

    /// <summary>Purchase by index into the consumableProducts list (for UI buttons).</summary>
    public void BuyProductByCatalogIndex(int index)
    {
        if (index < 0 || index >= consumableProducts.Count)
        {
            Debug.LogWarning("[IAP] Invalid product index: " + index);
            return;
        }
        BuyProductId(consumableProducts[index].storeProductId);
    }

    public void BuyProductId(string productId)
    {
        if (string.IsNullOrEmpty(productId))
        {
            return;
        }
        if (!IsReady())
        {
            Debug.LogWarning("[IAP] Store not ready yet.");
            return;
        }
        Product product = storeController.products.WithID(productId);
        if (product == null)
        {
            Debug.LogWarning("[IAP] Unknown product id: " + productId);
            return;
        }
        if (!product.availableToPurchase)
        {
            Debug.LogWarning("[IAP] Product not available: " + productId);
            return;
        }
        storeController.InitiatePurchase(product);
    }

    public void RestorePurchases()
    {
        if (!IsReady())
        {
            Debug.LogWarning("[IAP] Store not ready; cannot restore.");
            return;
        }
#if UNITY_IOS
        extensionProvider.GetExtension<IAppleExtensions>()?.RestoreTransactions((result, message) =>
        {
            Debug.Log("[IAP] Restore finished: " + result + (string.IsNullOrEmpty(message) ? "" : " — " + message));
        });
#else
        Debug.Log("[IAP] Restore is only needed on iOS; no-op on this platform.");
#endif
    }

    public string GetLocalizedPriceString(int catalogIndex)
    {
        if (!IsReady() || catalogIndex < 0 || catalogIndex >= consumableProducts.Count)
        {
            return string.Empty;
        }
        string id = consumableProducts[catalogIndex].storeProductId;
        Product p = storeController.products.WithID(id);
        return p?.metadata?.localizedPriceString ?? string.Empty;
    }

    public int GetGrantedGameYenForCatalogIndex(int catalogIndex)
    {
        if (catalogIndex < 0 || catalogIndex >= consumableProducts.Count)
        {
            return 0;
        }
        return consumableProducts[catalogIndex].grantedAmount;
    }

    public string GetFallbackDisplayPrice(int catalogIndex)
    {
        if (catalogIndex < 0 || catalogIndex >= consumableProducts.Count)
        {
            return string.Empty;
        }
        return consumableProducts[catalogIndex].fallbackDisplayPrice ?? string.Empty;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        isInitialized = true;
        Debug.Log("[IAP] Initialized.");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning("[IAP] OnInitializeFailed: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning("[IAP] OnInitializeFailed: " + error + " — " + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        if (!grantByProductId.TryGetValue(productId, out int grantedAmount) || grantedAmount <= 0)
        {
            Debug.LogWarning("[IAP] No grant mapping for product: " + productId);
            return PurchaseProcessingResult.Complete;
        }
        if (dataManager == null)
        {
            dataManager = FindObjectOfType<DataManager>();
        }
        if (dataManager == null)
        {
            Debug.LogError("[IAP] DataManager missing; cannot grant currency.");
            return PurchaseProcessingResult.Complete;
        }
        dataManager.AddMoneyFromPurchase(grantedAmount);
        Debug.Log("[IAP] Granted " + grantedAmount + " in-game currency for product " + productId);
        PurchaseSucceeded?.Invoke();
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        LogPurchaseFailed(product, failureReason, null);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        if (failureDescription == null)
        {
            LogPurchaseFailed(product, PurchaseFailureReason.Unknown, null);
            return;
        }
        LogPurchaseFailed(product, failureDescription.reason, failureDescription.message);
    }

    static void LogPurchaseFailed(Product product, PurchaseFailureReason reason, string message)
    {
        string id = product != null ? product.definition.id : "?";
        string tail = string.IsNullOrEmpty(message) ? reason.ToString() : reason + " — " + message;
        Debug.LogWarning("[IAP] Purchase failed: " + id + " — " + tail);
    }
}
