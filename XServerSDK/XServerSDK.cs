using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Net;
using System.Text; 
using System.IO; 
using UnityEngine.SceneManagement;
using SimpleJSON;

public class XServerSDK : MonoBehaviour {

	// PASTE YOUR DATABASE PATH HERE:
	public static String DATABASE_PATH = "YOUR_DATABASE_PATH";

	// SET THE NAME OF YOUR GAME
	public static String GAME_NAME = "XServer";



	// ------------------------------------------------
	// MARK: - GLOBAL STATIC VARIABLES
	// ------------------------------------------------
	public static String IOS_DEVICE_TOKEN = "";
	public static String ANDROID_DEVICE_TOKEN = "";
	public static String TABLES_PATH = DATABASE_PATH + "_Tables/";
	public static String pushMessage = "";
	public static String pushType = "";

	// ------------------------------------------------
	// MARK: - XServer -> COMMON ERROR MESSAGES
	// ------------------------------------------------
	public static String XS_ERROR = "No response from server. Try again later.";
	public static String E_101 = "Username already exists. Please choose another one.";
	public static String E_102 = "Email already exists. Please choose another one.";
	public static String E_103 = "Object not found.";
	public static String E_104 = "Something went wrong while sending a Push Notification.";
	public static String E_201 = "Something went wrong while creating/updating data.";
	public static String E_202 = "Either the username or password are wrong. Please type them again.";
	public static String E_203 = "Something went wrong while deleting data.";
	public static String E_301 = "Email doesn't exists in the database. Try a new one.";
	public static String E_302 = "You have signed in with a Social account, password cannot be changed.";
	public static String E_401 = "File upload failed. Try again";



	// -------------------------------------------------
	// ------------- XServer FUNCTIONS -----------------
	// -------------------------------------------------


	//-------------------------------------------
	// XSCurrentUser -> GET CURRENT USER'S DATA
	//-------------------------------------------
	public static JSONNode XSCurrentUser() {
		var currentUser = PlayerPrefs.GetString("currentUser", null);
		var cuObj = JSON.Parse("{}");

		if (currentUser != "") {
			var request = (HttpWebRequest)WebRequest.Create(TABLES_PATH + "m-query.php?");
			var postData = "tableName=Users";
			var data = Encoding.ASCII.GetBytes(postData);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = data.Length;
			using (var stream = request.GetRequestStream()){ stream.Write(data, 0, data.Length); }
			var response = (HttpWebResponse)request.GetResponse();
			var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
			
			// Debug.Log("XSCurrentUser -> RESPONSE: " + responseString);

			if (responseString == XS_ERROR) {
				simpleAlert(XS_ERROR);
				cuObj = null;
				PlayerPrefs.SetString("currentUser", null);

			} else {
				var users = JSON.Parse(responseString);
				var ok = false;
				
				// Search for currentUser obj
				if (users.Count != 0) {
					for (int u=0; u<users.Count; u++) {
						var uObj = users[u];
						
						if (uObj["ID_id"] == currentUser) {
							Debug.Log("** CURRENT USER: " + uObj["ST_username"] + " **");
							ok = true;
							cuObj = uObj;
						}

						// Object not found
						if (u == users.Count - 1 && !ok) {
							cuObj = null;
							PlayerPrefs.SetString("currentUser", null);
						}
					}// ./ For

				// Object not found
				} else { cuObj = null; PlayerPrefs.SetString("currentUser", null); }
			} //./ If

		// currentUser PlayerPrefs is null
		} else { cuObj = null; PlayerPrefs.SetString("currentUser", null); }

	return cuObj;
	}


	//-------------------------------------------
	// XSSignIn -> SIGN IN
	//-------------------------------------------
	public static Boolean XSSignIn(String username, String password) {
		var request = (HttpWebRequest)WebRequest.Create(TABLES_PATH + "m-query.php?");
		var postData = "tableName=Users";
		var data = Encoding.ASCII.GetBytes(postData);
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = data.Length;
		using (var stream = request.GetRequestStream()){ stream.Write(data, 0, data.Length); }
		var response = (HttpWebResponse)request.GetResponse();
		var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

		// Debug.Log("XSSignIn -> RESPONSE: " + responseString);

		var userResult = false;

		if (responseString == XS_ERROR) {
			simpleAlert(XS_ERROR);
			userResult = false;

			return false;

		} else {
			var users = JSON.Parse(responseString);
			var ok = false;
			
			// Search for currentUser obj
			if (users.Count != 0) {
				for (int u=0; u<users.Count; u++) {
					var uObj = users[u];
					
					if (uObj["ST_username"] == username && uObj["ST_password"] == password) {
						Debug.Log("** SIGNED IN AS: " + uObj["ST_username"] + " **");
						PlayerPrefs.SetString("currentUser", uObj["ID_id"]);
						ok = true;
						userResult = true;
					}

					// User doesn't exists in database or credentials are wrong
					if (u == users.Count - 1 && !ok) {
						userResult = false; simpleAlert(E_202);
					}
				}// ./ For

			// No users in the database!
			} else { userResult = false; simpleAlert(E_202); }

		return userResult;
		}
	}



	//-------------------------------------------
	// XSSignUp -> SIGN UP
	//-------------------------------------------
	public static String XSSignUp(String username, String password, String email, String signInWith) {
		var request = (HttpWebRequest)WebRequest.Create(TABLES_PATH + "m-signup.php?");
		var postData = "tableName=Users";
		postData += "&ST_username=" + username;
      	postData += "&ST_password=" + password;
      	postData += "&ST_email=" + email;
      	postData += "&signInWith=" +  signInWith;
      	postData += "&ST_iosDeviceToken=" + IOS_DEVICE_TOKEN;
      	postData += "&ST_androidDeviceToken=" + ANDROID_DEVICE_TOKEN;
      	
		var data = Encoding.ASCII.GetBytes(postData);
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = data.Length;
		using (var stream = request.GetRequestStream()){ stream.Write(data, 0, data.Length); }
		var response = (HttpWebResponse)request.GetResponse();
		var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

		Debug.Log("XSSignUp -> RESPONSE: " + responseString);
		
		string signUpresponse = "";

		if (responseString == "e_101") { signUpresponse = null; simpleAlert(E_101);
		} else if (responseString == "e_102") {  signUpresponse = null; simpleAlert(E_102);
        } else if (responseString == XS_ERROR) { signUpresponse = null; simpleAlert(XS_ERROR);
        } else {
        	signUpresponse = responseString;
        	String[] resultsArr = signUpresponse.Split('-');
        	String uID = resultsArr[0];
        	PlayerPrefs.SetString("currentUser", uID);
        }

	return signUpresponse;
	}


	//-------------------------------------------
	// XSQuery -> QUERY DATA
	//-------------------------------------------
	public static JSONNode XSQuery(String tableName, String columnName, String orderBy) {
    	var request = (HttpWebRequest)WebRequest.Create(TABLES_PATH + "m-query.php?");
		var postData = "tableName=" + tableName;
		postData += "&columnName=" + columnName;
		postData += "&orderBy=" + orderBy;
		var data = Encoding.ASCII.GetBytes(postData);
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = data.Length;
		using (var stream = request.GetRequestStream()) { stream.Write(data, 0, data.Length); }
		var response = (HttpWebResponse)request.GetResponse();
		var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
		
		// Debug.Log("XSQuery -> RESPONSE: " + responseString);

		// JSONNode objects
		var objects = JSON.Parse(responseString);

	return objects;
	}



	//-------------------------------------------
	// XSObject -> SAVE/UPDATE OBJECT
	//-------------------------------------------
	public static JSONNode XSObject(List<String> parameters){
		var request = (HttpWebRequest)WebRequest.Create(TABLES_PATH + "m-add-edit.php?");
		var data = Encoding.ASCII.GetBytes(String.Join("",parameters));
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = data.Length;
		using (var stream = request.GetRequestStream()){ stream.Write(data, 0, data.Length); }
		var response = (HttpWebResponse)request.GetResponse();
		var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

		// Debug.Log("XSObject -> RESPONSE: " + responseString);

		// JSONNode object
		var obj = JSON.Parse(responseString);
		
	return obj;
	}

	// ------------------------------------------------
	// MARK: - BUILD QUERY PARAMETERS STRING
	// ------------------------------------------------
	public static String param(String columnName, String value){
		var p = "";
		p += "&" + columnName + "=" + value;
		return p;
	}



	//-------------------------------------------
	// XSGetPointer -> GET OBJECT POINTER
	//-------------------------------------------
	public static JSONNode XSGetPointer(String tableName, String id) {
    	  var request = (HttpWebRequest)WebRequest.Create(TABLES_PATH + "m-query.php?");
	  var postData = "tableName=" + tableName;
	  var data = Encoding.ASCII.GetBytes(postData);
	  request.Method = "POST";
	  request.ContentType = "application/x-www-form-urlencoded";
	  request.ContentLength = data.Length;
	  using (var stream = request.GetRequestStream()) { stream.Write(data, 0, data.Length); }
	  var response = (HttpWebResponse)request.GetResponse();
	  var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

	  // Debug.Log("XSGetPointer -> RESPONSE: " + responseString);

	  var pointerObj = JSON.Parse("{}");

	  if (responseString == XS_ERROR) {
		simpleAlert(XS_ERROR);
		pointerObj = null;
	  } else {
		var objects = JSON.Parse(responseString);
		var ok = false;

		// Search for pointer object
		if (objects.Count != 0) {
			for (int p=0; p<objects.Count; p++) {
				var obj = objects[p];
				if (obj["ID_id"] == id) {
					ok = true;
					pointerObj = obj;
				}

				// Object not found
				if (p == objects.Count - 1 && !ok) {
					pointerObj = null;
					simpleAlert(E_103);
				}
			} // ./ For

		// Object not found
		} else { pointerObj = null; simpleAlert(E_103); }
	} //./ If

	return pointerObj;
	}


	//-------------------------------------------
	// XSRefreshObjectData -> REFRESH OBJECT'S DATA
	//-------------------------------------------
	public static JSONNode XSRefreshObjectData(String tableName, JSONNode jsonObj) {
    		var request = (HttpWebRequest)WebRequest.Create(TABLES_PATH + "m-query.php?");
		var postData = "tableName=" + tableName;
		var data = Encoding.ASCII.GetBytes(postData);
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = data.Length;
		using (var stream = request.GetRequestStream()) { stream.Write(data, 0, data.Length); }
		var response = (HttpWebResponse)request.GetResponse();
		var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

		// Debug.Log("XSRefreshObjectData -> RESPONSE: " + responseString);

		var refreshedObj = JSON.Parse("{}");

		if (responseString == XS_ERROR) {
			simpleAlert(XS_ERROR);
			refreshedObj = null;
		} else {
			var objects = JSON.Parse(responseString);
			for (int r=0; r<objects.Count; r++) {
				var obj = objects[r];
				if (obj["ID_id"] == jsonObj["ID_id"]) {
					refreshedObj = obj;
				}
			} // ./ For
		} //./ If
		
	return refreshedObj;
	}


	//-------------------------------------------
	// XSDelete -> DELETE AN OBJECT
	//-------------------------------------------
	public static Boolean XSDelete(String tableName, String id) {
		var request = (HttpWebRequest)WebRequest.Create(TABLES_PATH + "m-delete.php?");
		var postData = "tableName=" + tableName;
		postData += "&id=" + id;
		var data = Encoding.ASCII.GetBytes(postData);
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = data.Length;
		using (var stream = request.GetRequestStream()){ stream.Write(data, 0, data.Length); }
		var response = (HttpWebResponse)request.GetResponse();
		var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

		Debug.Log("XSDelete -> RESPONSE: " + responseString);
		
		var deleteResponse = false;

		if (responseString == XS_ERROR) { deleteResponse = false; simpleAlert(XS_ERROR);
		} else if(responseString == "deleted") { deleteResponse = true;
        } else { deleteResponse = false; simpleAlert(E_103); }

	return deleteResponse;
	}


	//-------------------------------------------
	// XSResetPassword -> RESET PASSWORD
	//-------------------------------------------
	public static String XSResetPassword(String email){
		var request = (HttpWebRequest)WebRequest.Create(TABLES_PATH + "forgot-password.php?email=" + email);
		request.Method = "GET";
		request.ContentType = "application/x-www-form-urlencoded";
		var response = (HttpWebResponse)request.GetResponse();
		var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

		var resetResponse = "";
		// Debug.Log("XSResetPassword -> RESPONSE: " + responseString);
		
		if (responseString ==  XS_ERROR) { resetResponse = null; simpleAlert(XS_ERROR);
		} else if (responseString ==  "e_301") { resetResponse = null; simpleAlert(E_301);
        } else if (responseString == "e_302") { resetResponse = null; simpleAlert(E_302);
        } else { resetResponse = responseString; }

	return resetResponse;
	}


	//-------------------------------------------
	// MARK - XSLogout -> LOGOUT
	//-------------------------------------------
	public static Boolean XSLogout() {
		PlayerPrefs.SetString("currentUser", null);
	return true;
	}



	//-------------------------------------------
	// XSSendiOSPush -> SEND iOS PUSH NOTIFICATION
	//-------------------------------------------
	public static Boolean XSSendiOSPush(String message, String deviceToken, String pushType) {
		var request = (HttpWebRequest)WebRequest.Create(DATABASE_PATH + "_Push/send-ios-push.php?");
		var postData = "message=" + message;
		postData += "&deviceToken=" + deviceToken;
		postData += "&pushType=" + pushType;
		var data = Encoding.ASCII.GetBytes(postData);
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = data.Length;
		using (var stream = request.GetRequestStream()){ stream.Write(data, 0, data.Length); }
		var response = (HttpWebResponse)request.GetResponse();
		var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

		Debug.Log("XSSendiOSPush -> RESPONSE: " + responseString);
		
		var pushResponse = false;

		if (responseString == "e_104") { pushResponse = false; simpleAlert(E_104);
		} else if(responseString == XS_ERROR) { pushResponse = false; simpleAlert(XS_ERROR);
        } else { pushResponse = true; }

	return pushResponse;
	}


	//-------------------------------------------
	// XSSendAndroidPush -> SEND ANDROID PUSH NOTIFICATION
	//-------------------------------------------
	public static Boolean XSSendAndroidPush(String message, String deviceToken, String pushType) {
		var request = (HttpWebRequest)WebRequest.Create(DATABASE_PATH + "_Push/send-android-push.php?");
		var postData = "message=" + message;
		postData += "&deviceToken=" + deviceToken;
		postData += "&pushType=" + pushType;
		var data = Encoding.ASCII.GetBytes(postData);
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = data.Length;
		using (var stream = request.GetRequestStream()){ stream.Write(data, 0, data.Length); }
		var response = (HttpWebResponse)request.GetResponse();
		var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

		Debug.Log("XSSendAndroidPush -> RESPONSE: " + responseString);
		
		var pushResponse = false;

		if (responseString == "e_104") { pushResponse = false; simpleAlert(E_104);
		} else if(responseString == XS_ERROR) { pushResponse = false; simpleAlert(XS_ERROR);
        } else { pushResponse = true; }

	return pushResponse;
	}



	//-------------------------------------------
	// MARK - XSUploadFile -> UPLOAD A FILE
	//-------------------------------------------
	public static IEnumerator XSUploadFile(byte[] bytes, String fileName, System.Action<String> callback) {
		WWWForm form = new WWWForm();
	    form.AddBinaryData("file", bytes, fileName, "");
	    form.AddField("fileName", fileName);

	    UnityWebRequest req = UnityWebRequest.Post(XServerSDK.DATABASE_PATH + "upload-file.php", form);
	    yield return req.SendWebRequest();
	    if (req.isHttpError || req.isNetworkError) {
	        Debug.Log(req.error);
	        // return null;
	        callback(null);
	        
	    } else {
	        StringBuilder sb = new StringBuilder();
            foreach (System.Collections.Generic.KeyValuePair<string, string> dict in req.GetResponseHeaders()){
                sb.Append(dict.Key).Append(": \t[").Append(dict.Value).Append("]\n");
            }
            // Debug.Log("XSUploadFile -> RESPONSE: " + DATABASE_PATH + req.downloadHandler.text);
            
            // return file URL
            callback(DATABASE_PATH + req.downloadHandler.text);
	    } //./ If
    }


	//-------------------------------------------
	// MARK - GET IMAGE FROM URL
	//-------------------------------------------
	public static IEnumerator GetImageFromURL(String fileURL, Image img){   
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(fileURL);
        yield return request.SendWebRequest();
        if(request.isNetworkError || request.isHttpError) {
            Debug.Log(request.error);
        } else {
            Texture2D texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
            Sprite mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            img.sprite = mySprite; 
        }
    }

	// ------------------------------------------------
	// MARK: - XSGetStringFromArray -> GET STRING FROM ARRAY
	// ------------------------------------------------
	public static String XSGetStringFromArray(List<String> arr){
		var arrayStr = String.Join(",", arr);
		return arrayStr;
	}


	//-----------------------------------------------
	// MARK - GET STRING FROM DATE
	//-----------------------------------------------
	public static String XSGetStringFromDate(DateTime date){
		var dateStr = String.Format("{0:yyyy-MM-dd'T'HH:mm:ss}", date);
		return dateStr;
	}

	//-----------------------------------------------
	// MARK - GET DATE FROM STRING
	//-----------------------------------------------
	public static DateTime XSGetDateFromString(String dateString){
		System.DateTime date = System.DateTime.Parse(dateString);
		return date;
	}




	//-----------------------------------------------
	// MARK - SIMPLE ALERT
	//-----------------------------------------------
	public static void simpleAlert(String message){
		if (EditorUtility.DisplayDialog(GAME_NAME, message, "Ok")){}
	}


} // ./ end
