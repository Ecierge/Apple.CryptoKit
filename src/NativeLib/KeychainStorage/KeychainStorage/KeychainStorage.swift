//
//  KeychainStorage.swift
//  KeychainStorage
//
//  Created by Oleksii Bailo on 10/4/24.
//

import Foundation
import Security


@objc(ACKeychainStorage)
public class ACKeychainStorage : NSObject {
    
    @objc(isKeyExist:isPublic:)
    public static func isKeyExist(keyName: String, isPublic: Bool) -> Bool {

        let existingKey = findKey(keyName: keyName, isPublic: isPublic)
        if existingKey != nil {
            return true
        }

        return false;
    }
    
    @objc(getKey:isPublic:error:)
    public static func getKey(tag: String, isPublic: Bool) throws -> Data {

        //Searching existing key
        let existingKey = findKey(keyName: tag, isPublic: isPublic)
        if existingKey != nil {
            return try exportKey(secKey:existingKey!, isPublic:isPublic)
        }
        
        throw NSError(domain: NSOSStatusErrorDomain, code: Int(errSecItemNotFound), userInfo: [
            NSLocalizedDescriptionKey: "Key '\(tag)' not found in keychain"
        ])
    }
    
    @objc(createRSAKeyAnsStoreInkeychain:algorithm:keySize:isKeyPublic:keyUsage:overwrite:error:)
    public static func createRSAKeyAnsStoreInkeychain(tag: String, algorithm: String, keySize: Int, isPublic: Bool, keyUsage: Bool, overwrite: Bool) throws -> Data {

        let tagData = tag.data(using: .utf8)!
        var keyType = kSecAttrKeyTypeRSA;
        
        switch algorithm {
        case "rsa":
            print("RSA algorithm selected")
        case "ec":
            keyType = kSecAttrKeyTypeEC
            print("Elliptic Curve algorithm selected")
//        case "dsa":
//            keyType = kSecAttrKeyTypeDSA
//            print("DSA algorithm selected")
        default:
            print("Unknown algorithm")
            throw NSError(domain: NSOSStatusErrorDomain, code: Int(errSecUnimplemented), userInfo: [
                NSLocalizedDescriptionKey: "Unknown algorithm: \(algorithm)"
            ])
        }
        
        if (overwrite)
        {
            // Delete the existing key if it exists
            let deleteQuery: [String: Any] = [
                kSecClass as String: kSecClassKey,
                kSecAttrApplicationTag as String: tagData
            ]
            SecItemDelete(deleteQuery as CFDictionary) // Ignore error if key doesn't exist
        }

        //Searching existing key
        let existingKey = findKey(keyName: tag, isPublic: isPublic)
        if existingKey != nil {
            return try exportKey(secKey:existingKey!, isPublic:isPublic)
        }
        
        //Key not found, let's try create and store new one
        var attributes: [String: Any] = [
            kSecAttrKeyType as String: keyType,             // Key type (RSA, EC, etc.)
            kSecAttrKeySizeInBits as String: keySize,       // Key size
            kSecAttrLabel as String: tag,                   // Label for the key
            kSecAttrApplicationTag as String: tagData,      // Custom tag for identifying the key
            kSecAttrIsPermanent as String: true,            // Store the key permanently in the Keychain
            kSecAttrAccessible as String: kSecAttrAccessibleAfterFirstUnlock,  // Required for Mac Catalyst
        ]

        if !isPublic {
            //Private key attributes
            var privateKeyAttributes: [String: Any] = [
                kSecAttrIsPermanent as String: true,            // Store the key permanently in the Keychain
                kSecAttrApplicationTag as String: tagData,      // Custom tag for identifying the key
            ]

            if (keyUsage) {
                privateKeyAttributes[kSecAttrCanDecrypt as String] = true   // Specify decryption usage
            } else {
                privateKeyAttributes[kSecAttrCanSign as String] = true      // Specify usage for signing (if needed)
            }
            attributes[kSecPrivateKeyAttrs as String] = privateKeyAttributes
        }

        //Finally generate the key
        var error: Unmanaged<CFError>?
        guard let newKey = SecKeyCreateRandomKey(attributes as CFDictionary, &error) else {
            let cfError = error!.takeRetainedValue() as Error
            let nsError = cfError as NSError
            print("Error generating key: \(nsError)")
            throw nsError
        }
        
        return try exportKey(secKey:newKey, isPublic:isPublic)
    }

    //
    static func findKey(keyName: String, isPublic: Bool) -> SecKey? {

        var keyQuery: [String: Any] = [
            kSecClass as String: kSecClassKey,
            kSecAttrApplicationTag as String: keyName.data(using: .utf8)!,
            kSecReturnRef as String: true,
            kSecMatchLimit as String: kSecMatchLimitOne
        ]

        keyQuery[kSecAttrKeyClass as String] = isPublic ? kSecAttrKeyClassPublic : kSecAttrKeyClassPrivate;
        
        var item: CFTypeRef?
        let status = SecItemCopyMatching(keyQuery as CFDictionary, &item)
        if status == errSecSuccess {
            if let keyRef = item, CFGetTypeID(keyRef) == SecKeyGetTypeID() {
                return (keyRef as! SecKey)
            }
        }

        print("Key not found: \(status)")
        return nil;
    }

    static func exportKey(secKey: SecKey, isPublic: Bool) throws -> Data {
        // Export the key
        var error: Unmanaged<CFError>?
        if isPublic {
            guard let publicKey = SecKeyCopyPublicKey(secKey) else {
                throw NSError(domain: NSOSStatusErrorDomain, code: Int(errSecParam), userInfo: [
                    NSLocalizedDescriptionKey: "Failed to get public key from SecKey"
                ])
            }
            if let publicKeyData = SecKeyCopyExternalRepresentation(publicKey, &error) {
                return publicKeyData as Data
            }
        } else {
            if let privateKeyData = SecKeyCopyExternalRepresentation(secKey, &error) {
                return privateKeyData as Data
            }
        }

        if let cfError = error {
            let nsError = cfError.takeRetainedValue() as Error as NSError
            print("Error exporting key: \(nsError)")
            throw nsError
        }
        
        throw NSError(domain: NSOSStatusErrorDomain, code: Int(errSecParam), userInfo: [
            NSLocalizedDescriptionKey: "Failed to export key"
        ])
    }
}
