using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BuyItemControl : MonoBehaviour {

    public int index = 1;
    public GameObject priceText;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        //if (PoolGameManager.Instance.IAPControl.controller != null) {
        //    if (this.index == 1) {
        //        priceText.GetComponent<Text>().text = PoolGameManager.Instance.IAPControl.controller.products.WithID(PoolGameManager.Instance.IAPControl.SKU_1000_COINS).metadata.localizedPriceString;
        //    } else if (this.index == 2) {
        //        priceText.GetComponent<Text>().text = PoolGameManager.Instance.IAPControl.controller.products.WithID(PoolGameManager.Instance.IAPControl.SKU_5000_COINS).metadata.localizedPriceString;
        //    } else if (this.index == 3) {
        //        priceText.GetComponent<Text>().text = PoolGameManager.Instance.IAPControl.controller.products.WithID(PoolGameManager.Instance.IAPControl.SKU_10000_COINS).metadata.localizedPriceString;
        //    } else if (this.index == 4) {
        //        priceText.GetComponent<Text>().text = PoolGameManager.Instance.IAPControl.controller.products.WithID(PoolGameManager.Instance.IAPControl.SKU_50000_COINS).metadata.localizedPriceString;
        //    } else if (this.index == 5) {
        //        priceText.GetComponent<Text>().text = PoolGameManager.Instance.IAPControl.controller.products.WithID(PoolGameManager.Instance.IAPControl.SKU_100000_COINS).metadata.localizedPriceString;
        //    }
        //}

    }

    public void buyItem() {
        //PoolGameManager.Instance.IAPControl.OnPurchaseClicked(index);
    }
}
