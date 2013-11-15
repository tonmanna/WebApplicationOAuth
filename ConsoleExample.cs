﻿// Copyright 2012, Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Author: api.anash@gmail.com (Anash P. Oommen)

using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.v201306;
using Google.Api.Ads.Common.Lib;

using System;
using System.Data;

namespace Google.Api.Ads.AdWords.Examples.CSharp.OAuth {
  /// <summary>
  /// This code example shows how to run an AdWords API command line application
  /// while incorporating the OAuth2 installed application flow into your
  /// application. If your application uses a single MCC login to make calls to
  /// all your accounts, you shouldn't use this code example. Instead, you
  /// should run Common\Util\OAuth2TokenGenerator.cs to generate a refresh token
  /// and set that in user.Config.OAuth2RefreshToken field, or set
  /// OAuth2RefreshToken key in your App.config / Web.config.
  ///
  /// This code example depends on Console environment only for reading and
  /// writing values, you may use this code example in other environments like
  /// Windows Form applications with minimial modifications.
  ///
  /// To run this application,
  ///
  /// 1. You should create a new Console Application project.
  /// 2. Add reference to the following assemblies:
  /// <list type="table">
  /// <item>Google.Ads.Common.dll</item>
  /// <item>Google.Ads.OAuth.dll</item>
  /// <item>Google.AdWords.dll</item>
  /// <item>System.Web</item>
  /// <item>System.Configuration</item>
  /// </list>
  /// 3. Replace the Main() method with this class's method.
  /// 4. Copy App.config from AdWords.Examples.CSharp project, and configure
  /// it as shown in ths project's Web.config.
  /// 5. Compile and run this example.
  /// </summary>
  public class ConsoleExample {
    /// <summary>
    /// The main method.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    static void Main(string[] args) {
      AdWordsUser user = new AdWordsUser();
      AdWordsAppConfig config = (user.Config as AdWordsAppConfig);
      if (config.AuthorizationMethod == AdWordsAuthorizationMethod.OAuth2) {
        if (config.OAuth2Mode == OAuth2Flow.APPLICATION &&
            string.IsNullOrEmpty(config.OAuth2RefreshToken)) {
          DoAuth2Authorization(user);
        }
      } else {
        throw new Exception("Authorization mode is not OAuth.");
      }

      Console.Write("Enter the customer id: ");
      string customerId = Console.ReadLine();
      config.ClientCustomerId = customerId;

      // Get the CampaignService.
      CampaignService campaignService =
          (CampaignService) user.GetService(AdWordsService.v201306.CampaignService);

      // Create the selector.
      Selector selector = new Selector();
      selector.fields = new string[] {"Id", "Name", "Status"};

      // Set the selector paging.
      selector.paging = new Paging();

      int offset = 0;
      int pageSize = 500;

      CampaignPage page = new CampaignPage();

      try {
        do {
          selector.paging.startIndex = offset;
          selector.paging.numberResults = pageSize;

          // Get the campaigns.
          page = campaignService.get(selector);

          // Display the results.
          if (page != null && page.entries != null) {
            int i = offset;
            foreach (Campaign campaign in page.entries) {
              Console.WriteLine("{0}) Campaign with id = '{1}', name = '{2}' and status = '{3}'" +
                " was found.", i + 1, campaign.id, campaign.name, campaign.status);
              i++;
            }
          }
          offset += pageSize;
        } while (offset < page.totalNumEntries);
        Console.WriteLine("Number of campaigns found: {0}", page.totalNumEntries);
      } catch (Exception ex) {
        throw new System.ApplicationException("Failed to retrieve campaigns", ex);
      }
    }

    /// <summary>
    /// Does the OAuth2 authorization for installed applications.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    private static void DoAuth2Authorization(AdWordsUser user) {
      // Since we are using a console application, set the callback url to null.
      user.Config.OAuth2RedirectUri = null;
      AdsOAuthProviderForApplications oAuth2Provider =
          (user.OAuthProvider as AdsOAuthProviderForApplications);
      // Get the authorization url.
      string authorizationUrl = oAuth2Provider.GetAuthorizationUrl();
      Console.WriteLine("Open a fresh web browser and navigate to \n\n{0}\n\n. You will be " +
          "prompted to login and then authorize this application to make calls to the " +
          "AdWords API. Once approved, you will be presented with an authorization code.",
          authorizationUrl);

      // Accept the OAuth2 authorization code from the user.
      Console.Write("Enter the authorization code :");
      string authorizationCode = Console.ReadLine();

      // Fetch the access and refresh tokens.
      oAuth2Provider.FetchAccessAndRefreshTokens(authorizationCode);
    }
  }
}
