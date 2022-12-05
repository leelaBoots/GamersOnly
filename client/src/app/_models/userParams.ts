import { User } from "./user";

export class UserParams {
  // using a class here allows us to have a constructor to init properties when we create it
  gender: string;
  minAge = 18;
  maxAge = 99;
  pageNumber = 1;
  pageSize = 5;
  orderBy = 'lastActive';

  constructor(user: User) {
    this.gender = user.gender === 'female' ? 'male' : 'female'
  }

}