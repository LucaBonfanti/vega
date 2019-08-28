import { JwtHelper } from 'angular2-jwt';
// app/auth.service.ts

import { Injectable }      from '@angular/core';
import { tokenNotExpired } from 'angular2-jwt';

// Avoid name not found warnings
import Auth0Lock from 'auth0-lock';
import auth0 from 'auth0-js';
import { Router } from '@angular/router';

@Injectable()
export class Auth {
  profile: any;
  private roles: string[] = []; 

  // Configure Auth0
  // lock = new Auth0Lock('RfRu3un13aOO73C7X2mH41qxfHRbUc33', 'vegaproject.auth0.com', {});
  /* lock = new Auth0Lock('3G4y5CLAgerL9pmoUOxKjBaaCGKryKPj', 'ellebi.eu.auth0.com', {
      auth: {
        redirectUrl: 'http://localhost:5000/',
        responseType: 'token id_token',
        audience: 'https://' + 'ellebi.eu.auth0.com' + '/userinfo',
        params: {
          scope: 'openid'
        }
      }
    }); */

    requestedScopes: string = 'openid profile read:timesheets create:timesheets';

    webAuth = new auth0.WebAuth({
      domain: 'ellebi.eu.auth0.com',
      clientID: '3G4y5CLAgerL9pmoUOxKjBaaCGKryKPj',
      responseType: 'token id_token',
      audience: 'https://' + 'ellebi.eu.auth0.com' + '/userinfo',
      scope: this.requestedScopes,  // 'openid',
      redirectUri: 'http://localhost:5000/'
    });

  constructor(public router: Router) {
    this.readUserFromLocalStorage();
    // this.lock.on("authenticated", (authResult) => this.onUserAuthenticated(authResult));
  
    const query = window.location.search;
    const shouldParseResult = (query.includes("code=") && query.includes("state="));
    if (window.location.hash) {
      try {
        this.handleAuthentication();
        console.log("Logged in!");
      } catch (err) {
        console.log("Error parsing redirect:", err);
      }
      // window.history.replaceState({}, document.title, "/");
    }
  }

  /* 
  private onUserAuthenticated(authResult) {
    console.log(authResult);
    localStorage.setItem('token', authResult.accessToken);

    this.lock.getUserInfo(authResult.accessToken, (error, profile) => {
      if (error)
        throw error;

      localStorage.setItem('profile', JSON.stringify(profile));

      this.readUserFromLocalStorage();
    });
  } */

  private readUserFromLocalStorage() {
    this.profile = JSON.parse(localStorage.getItem('profile'));

    var token = localStorage.getItem('token');
    if (token) {
      // var jwtHelper = new JwtHelper();
      // var decodedToken = jwtHelper.decodeToken(token);
      // this.roles = decodedToken['https://vega.com/roles'] || [];   TODO  manage roles 
    }
  }

  public isInRole(roleName) {
    return this.roles.indexOf(roleName) > -1;
  }

  public login(): void {
    // Call the show method to display the widget.
    // this.lock.show();
    var resultAuth = this.webAuth.authorize();
    console.log(resultAuth);
  }

  public authenticated() {
    // Check if there's an unexpired JWT
    // This searches for an item in localStorage with key == 'token'
    return this.isAuthenticated(); // tokenNotExpired('token');
  }

  public logout(): void {
    // Remove token from localStorage
    localStorage.removeItem('token');
    localStorage.removeItem('profile');
    // Remove tokens and expiry time from localStorage
    localStorage.removeItem('access_token');
    localStorage.removeItem('id_token');
    localStorage.removeItem('expires_at');
    localStorage.removeItem('scopes');

    this.profile = null;
    this.roles = [];
    // Go back to the home route
    this.router.navigate(['/']);
  }

  public handleAuthentication(): void {
    this.webAuth.parseHash({ hash: window.location.hash }, (err, authResult) => {
      console.log("authResult");
      console.log(authResult);

      if (authResult && authResult.accessToken) {
        window.location.hash = '';
        // this.onUserAuthenticated(authResult);
        this.setSession(authResult);
      } else if (err) {
          console.log(err);
          alert( 'Error: ' + err.error + '. Check the console for further details.' );
          this.router.navigate(['/']);
      }
    });
  }


  private setSession(authResult): void {
    console.log("setSession:");
    console.log(authResult);
    localStorage.setItem('token', authResult.idToken);
    // localStorage.setItem('profile', JSON.stringify(profile));   OLD  Lock.js

    // Set the time that the Access Token will expire at
    const expiresAt = JSON.stringify((authResult.expiresIn * 1000) + new Date().getTime());

    // If there is a value on the scope param from the authResult, use it to set scopes in the session for the user. 
    // Otherwise use the scopes as requested. If no scopes were requested, set it to nothing
    const scopes = authResult.scope || this.requestedScopes || '';

    this.webAuth.client.userInfo(authResult.accessToken, (err, user) => {
      console.log(user);
      localStorage.setItem('user', JSON.stringify(user));
    });
    localStorage.setItem('access_token', authResult.accessToken);
    localStorage.setItem('id_token', authResult.idToken);
    localStorage.setItem('expires_at', expiresAt);
    localStorage.setItem('scopes', JSON.stringify(scopes));
  }

  public isAuthenticated(): boolean {
    // Check whether the current time is past the
    // Access Token's expiry time
    const expiresAt = JSON.parse(localStorage.getItem('expires_at'));
    // console.log("isAuth  exp="+expiresAt+"  "+(new Date().getTime()))
    return new Date().getTime() < expiresAt;
  }

  public userHasScopes(scopes: Array<string>): boolean {
    const grantedScopes = JSON.parse(localStorage.getItem('scopes')).split(' ');
    return scopes.every(scope => grantedScopes.includes(scope));
  }

}