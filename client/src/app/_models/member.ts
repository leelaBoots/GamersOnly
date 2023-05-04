import { Photo } from "./photo";

// im using "username" instead of "userName" which is used in the tutorial, i tried to change it to match but was not able to fix errors so i left as is.

export interface Member {
    id: number;
    username: string;
    photoUrl: string;
    age: number;
    knownAs: string;
    created: Date;
    lastActive: Date;
    gender: string;
    introduction: string;
    lookingFor: string;
    interests: string;
    city: string;
    country: string;
    photos: Photo[];
}

