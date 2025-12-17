/*using System.Collections;
using UnityEngine;
using TMPro;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase;
using UnityEditor.Search;

public class EmailPassLogin : MonoBehaviour
{
    #region variables
    [Header("Login")]
    public TMP_InputField LoginEmail;
    public TMP_InputField loginPassword;

    [Header("Sign up")]
    public TMP_InputField SignupEmail;
    public TMP_InputField SignupPassword;
    public TMP_InputField SignupPasswordConfirm;

    [Header("Extra")]
    public GameObject loadingScreen;
    public TextMeshProUGUI logTxt;
    public GameObject loginUi, signupUi, SuccessUi;
    #endregion

    #region signup 
    public void SignUp()
    {
        loadingScreen.SetActive(true);

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        string email = SignupEmail.text;
        string password = SignupPassword.text;

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            loadingScreen.SetActive(false);

            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Sign up error: " + task.Exception);
                showLogMsg("Sign up failed.");
                return;
            }

            AuthResult result = task.Result;
            SignupEmail.text = "";
            SignupPassword.text = "";
            SignupPasswordConfirm.text = "";

            if (result.User.IsEmailVerified)
            {
                showLogMsg("Sign up Successful");
            }
            else
            {
                showLogMsg("Please verify your email!!");
                SendEmailVerification();
            }
        });
    }

    public void SendEmailVerification()
    {
        StartCoroutine(SendEmailForVerificationAsync());
    }

    IEnumerator SendEmailForVerificationAsync()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(() => sendEmailTask.IsCompleted);

            if (sendEmailTask.Exception != null)
            {
                Debug.Log("Email verification failed: " + sendEmailTask.Exception);
                showLogMsg("Email verification failed.");
            }
            else
            {
                showLogMsg("Verification email sent!");
            }
        }
    }
    #endregion

    #region login
    public void Login()
    {
        loadingScreen.SetActive(true);

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        string email = LoginEmail.text;
        string password = loginPassword.text;

        Credential credential = EmailAuthProvider.GetCredential(email, password);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            loadingScreen.SetActive(false);

            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Login failed: " + task.Exception);
                showLogMsg("Login failed.");
                return;
            }

            AuthResult result = task.Result;

            if (result.User.IsEmailVerified)
            {
                showLogMsg("Login Successful");
                loginUi.SetActive(false);
                SuccessUi.SetActive(true);
                SuccessUi.transform.Find("Desc").GetComponent<TextMeshProUGUI>().text = "Id: " + result.User.UserId;
            }
            else
            {
                showLogMsg("Please verify email!!");
            }
        });
    }
    #endregion

    #region UI actions

    // Called from a "Return to Sign In" button
    public void ShowLoginPanel()
    {
        signupUi.SetActive(false);
        loginUi.SetActive(true);
    }
   public void ShowSignUpPanel()
    {
        signupUi.SetActive(true);
        loginUi.SetActive(false);
    }

    // Called from a "Close" button to hide all UIs
    public void CloseAllPanels()
    {
        loginUi.SetActive(false);
        signupUi.SetActive(false);
        SuccessUi.SetActive(false);
    }

    #endregion

    #region extras
    void showLogMsg(string msg)
    {
        logTxt.text = msg;
        logTxt.GetComponent<Animation>().Play("textFadeout");
    }
    #endregion
}
*/